﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ScreenLib;

namespace StatusConsole {
    public class HostedCmdlineApp : IHostedService {

        public readonly IConfigurableServices _myServices;
        private readonly Dictionary<string, ITtyService> uarts;
        private Task guiInputHandler;
        public ITtyService uartInFocus { get; set; }
        private Screen appLogScreen;
        

        // Constructor called by IOC with injected ServiceCollection and Config
        public HostedCmdlineApp(IConfigurableServices service, IConfiguration conf) {
            _myServices = service ?? throw new ArgumentNullException(nameof(service));
            uarts = _myServices.GetTtyServices();
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            int mainX = 0;
            int mainY = 0;
            List<Screen> screens = new List<Screen>();
            foreach(var uart in uarts) {
                IConfigurationSection cs = uart.Value.GetScreenConfig();
                int width = cs.GetValue("Width", 80);
                int heigth = cs.GetValue("Height", 10);
                int posX = cs.GetValue("Pos_X", 0);
                int posY = cs.GetValue("Pos_Y", 0);
                if(posX + width > mainX) {
                    mainX = posX + width;
                }
                if(posY + heigth > mainY) {
                    mainY = posY + heigth;
                }
            }

            MainScreen<HostedCmdlineApp> main = new StatusConsoleMainView(mainX, mainY+14, this);
            foreach(var uart in uarts) {
                IConfigurationSection cs = uart.Value.GetScreenConfig();
                int width = cs.GetValue("Width", 80);
                int heigth = cs.GetValue("Height", 10);
                int posX = cs.GetValue("Pos_X", 0);
                int posY = cs.GetValue("Pos_Y", 0);
                ConsoleColor txtCol = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), cs.GetValue("Text", "White"));
                ConsoleColor bkgCol = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), cs.GetValue("Background", "Black"));
                
                
                Screen scr = main.AddScreen(posX, posY, new Screen(width, heigth));
                scr.HoriType = HorizontalType.WRAP;
                scr.VertType = VerticalType.WRAP_AROUND;
                scr.TextColor = txtCol;
                scr.BackgroundColor = bkgCol;
                uart.Value.SetScreen(scr);
                scr.Fill();
                await uart.Value.StartAsync(cancellationToken);
            }

            appLogScreen = main.AddScreen(0, mainY, new Screen(mainX, 12, ConsoleColor.DarkBlue, ConsoleColor.White));
            appLogScreen.Fill();
            


            appLogScreen.WriteLine("Use TAB to switch input control.");

            // prepare Input cursor         
            guiInputHandler = Task.Run(() => main.HandleConsoleInput(appLogScreen));

            uartInFocus = _myServices.GetCurrentService();
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            appLogScreen.WriteLine("Stopping App");
            foreach(var uart in uarts) {
                await uart.Value.StopAsync(cancellationToken);
            }
        }


    }
}
