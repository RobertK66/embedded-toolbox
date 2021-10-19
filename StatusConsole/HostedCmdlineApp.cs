using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ScreenLib;

namespace StatusConsole {
    public class HostedCmdlineApp : IHostedService {

        public readonly IUartServices _myServices;
        private readonly Dictionary<string, IUartService> uarts;
        private Task guiInputHandler;
        public IUartService uartInFocus { get; set; }

        private Screen inputScreen;


        // Constructor with IOC injection
        public HostedCmdlineApp(IUartServices service, IConfiguration conf) {
            _myServices = service ?? throw new ArgumentNullException(nameof(service));
            uarts = service.GetUartServices();

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

                Screen scr = main.AddScreen(posX, posY, new Screen(width, heigth));
                scr.HoriType = HorizontalType.WRAP;
                scr.VertType = VerticalType.WRAP_AROUND;
                uart.Value.SetScreen(scr);

                await uart.Value.StartAsync(cancellationToken);
            }

            inputScreen = main.AddScreen(0, mainY, new Screen(100, 14, ConsoleColor.DarkBlue, ConsoleColor.White));
            inputScreen.Clear();
            


            inputScreen.WriteLine("Hi starting App");

            // prepare Input cursor
            //Console.BackgroundColor = inputScreen.BackgroundColor;
            //Console.ForegroundColor = inputScreen.TextColor;
            //Console.SetCursorPosition(10, 14);
            guiInputHandler = Task.Run(() => main.HandleConsoleInput(inputScreen));


            uartInFocus = _myServices.GetCurrentService();
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            inputScreen.WriteLine("Hi stopping App");
            foreach(var uart in uarts) {
                await uart.Value.StopAsync(cancellationToken);
            }
        }


    }
}
