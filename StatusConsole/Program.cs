using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using ConsoleGUI.Controls;
using ConsoleGUI.Space;
using ConsoleGUI;
using ConsoleGUI.Input;
using ConsoleGUI.Data;
using StatusConsole.Controls;
using ConsoleGUI.Api;

namespace StatusConsole {

    public class Program : IHostedService
    {
        public async static Task<int> Main(string[] args) { 
            var host =  Host.CreateDefaultBuilder()
                       .ConfigureAppConfiguration((hbc, cb) => {
                           // Commandline pars with "--<anyname> <value>" automatically appear as "anyname" pars in the flattened config.
                           // to also use "-<anyname> <value>" you have to provide the specific mappings here
                           // The example mypar now works with either -mypar ode --mypar !!!
                           Dictionary<string, string> mappings = new Dictionary<string, string>() {
                                { "-mypar", "mypar" } };
                           cb.AddCommandLine(args, mappings);
                       })
                       .ConfigureLogging((cl) => {
                           cl.ClearProviders();    // This avoids logging output to console.
                                                   // TODO: add log stream to own screen later.....
                           cl.AddDebug();          // This gives Logging in the Debug Console of VS. (configure in appsettings.json)
                       })
                       .ConfigureServices(services => {
                           services.AddTransient<IConfigurableServices, MyServiceCollection>();
                           services.AddHostedService<Program>();
                       });

            await host.RunConsoleAsync();

            // This lines are not reached if Environment.Exit() is used somewhere in Services.....
            Console.WriteLine("Exit Code in Main[]:" + Environment.ExitCode);
            return Environment.ExitCode;      
        }

        // Program Instance part
        private readonly ILogger<Program> Log;
        private readonly IConfigurableServices uartServices;

        // T(G)UI
        private IControl mainwin = null;
        private TabPanel tabPanel = new();
        private MyInputController? myInputController = null;
        private MyFunctionController? myFunctionController = null;
        private int mainX = 0;
        private int mainY = 0;

        public Color GetGuiColor(ConsoleColor color) {
            if (color == ConsoleColor.DarkGray) return new Color(128, 128, 128);
            if (color == ConsoleColor.Gray) return new Color(192, 192, 192);
            int index = (int)color;
            byte d = ((index & 8) != 0) ? (byte)255 : (byte)128;
            return new Color(
                ((index & 4) != 0) ? d : (byte)0,
                ((index & 2) != 0) ? d : (byte)0,
                ((index & 1) != 0) ? d : (byte)0);
        }

        public Program(IConfiguration conf, ILogger<Program> logger , IConfigurableServices services) {
            Log = logger;
            Log.LogDebug("Program() Constructor called.");

            uartServices = services;
            myInputController = new MyInputController();
            myFunctionController = new MyFunctionController(services);
      
            LogPanel myLogPanel = new();
            bool tabAvailable = false;
            foreach (var uartService in services) {
                
                string name = uartService.GetInterfaceName();
                IConfigurationSection cs = uartService.GetScreenConfig();
                
                int width = cs.GetValue("Width", 80);
                int heigth = cs.GetValue("Height", 10);
                if (width > mainX) {
                    mainX = width;
                    
                }
                if (heigth > mainY) {
                    mainY = heigth;
                }
                
                ConsoleColor cc = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), cs.GetValue("Background", "Black"));
                Color backgroundColor = GetGuiColor(cc);
                cc = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), cs.GetValue("Text", "Whilte"));
                Color textColor = GetGuiColor(cc);

                var uartScreen = new MyUartScreen();
                TextBox textBox = new TextBox();
                uartService.SetScreen(uartScreen);

                tabPanel.AddTab(name, new Boundary {
                    Width = width,
                    MinHeight = heigth,
                    Content = new Background {
                        Color = backgroundColor,
                        Content = new DockPanel {
                            Placement = DockPanel.DockedControlPlacement.Bottom,
                            DockedControl = new Boundary {
                                MaxHeight = 1,
                                MinHeight = 1,
                                Content = textBox
                            },
                            FillingControl = uartScreen
                        }
                    }
                }, backgroundColor, new Color(128, 128, 128), textColor);
                tabAvailable = true;
                myInputController.AddCommandLine(textBox, CommandCallback);
                myFunctionController.AddUartScreen(name, uartScreen);
                Log.LogDebug($"Screen with {width}x{heigth} added -> {mainX}x{mainY}");
            }

            if (tabAvailable) {
                tabPanel.TabSwitched += TabPanel_TabSwitched;
                tabPanel.SelectTab(0);
            }

            mainwin = new DockPanel {
                Placement = DockPanel.DockedControlPlacement.Bottom,
                DockedControl = new Boundary {
                    MaxHeight = 10,
                    MinHeight = 10,
                    Content = myLogPanel
                },
                FillingControl = tabPanel
            };

        }

        private void TabPanel_TabSwitched(object sender, TabSwitchedArgs e) {
            myInputController.SetActive(e.selectedIdx);
            uartServices.SwitchCurrentService(e.selectedIdx);
            
        }

        private Thread? tuiThread;
        private IInputListener[]? input;

        public async Task StartAsync(CancellationToken cancellationToken) {
            Log.LogDebug("Program StartAsync called");

            ConsoleManager.Console = new SimplifiedConsole();
            
            ConsoleManager.Setup();
            ConsoleManager.Resize(new Size(mainX, mainY+16));
            ConsoleManager.Content = mainwin;

            input = new IInputListener[] {
                tabPanel,
                myInputController,
                myFunctionController
              };

            tuiThread = new Thread(new ThreadStart(TuiThread));
            tuiThread.Start();

            foreach (var uartService in uartServices) {
                await uartService.StartAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Log.LogDebug("Program StopAsync called");
            return Task.CompletedTask;
        }

        private void TuiThread() {
            try {
                Log.LogDebug("TUI Thread started");
                while (true) {
                    ConsoleManager.ReadInput(input);
                    Thread.Sleep(20);
                }
            } catch (ThreadInterruptedException) {
                Log.LogDebug("TUI Thread canceled by InterruptException");
            }
        }

        private void CommandCallback(string command) {
            Log.LogDebug("Command " + command);
            var s = uartServices.GetCurrentService();
            s.SendUart(command);
        }

    }

}
