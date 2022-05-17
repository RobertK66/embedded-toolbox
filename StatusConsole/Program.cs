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
                           services = services.AddTransient<IConfigurableServices, MyServiceCollection>();
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


        // GUI
        private IControl mainwin = null;
        private TabPanel tabPanel = new();
        //private static LogPanel myLogPanel = new();
        private TextBox myInputBox = new TextBox();
        private MyInputController? myInputLine = null;
        private int mainX = 0;
        private int mainY = 0;


        public static System.Drawing.Color FromColor(System.ConsoleColor c) {
            int cInt = (int)c;

            int brightnessCoefficient = ((cInt & 8) > 0) ? 2 : 1;
            int r = ((cInt & 4) > 0) ? 64 * brightnessCoefficient : 0;
            int g = ((cInt & 2) > 0) ? 64 * brightnessCoefficient : 0;
            int b = ((cInt & 1) > 0) ? 64 * brightnessCoefficient : 0;

            return System.Drawing.Color.FromArgb(r, g, b);
        }


        public Program(IConfiguration conf, ILogger<Program> logger , IConfigurableServices services) {
            Log = logger;
            Log.LogDebug("Program() Constructor called.");

            uartServices = services;

            myInputLine = new MyInputController(myInputBox, CommandCallback);
            LogPanel myLogPanel = new();
            bool tabAvailable = false;
            foreach (var uartService in services) {
                
                string name = uartService.GetInterfaceName();
                IConfigurationSection cs = uartService.GetScreenConfig();
                
                int width = cs.GetValue("Width", 80);
                int heigth = cs.GetValue("Height", 10);
                //int posX = cs.GetValue("Pos_X", 0);
                //int posY = cs.GetValue("Pos_Y", 0);
                if (width > mainX) {
                    mainX = width;
                    
                }
                if (heigth > mainY) {
                    mainY = heigth;
                }
                
                ConsoleColor cc = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), cs.GetValue("Background", "Black"));
                System.Drawing.Color sc = FromColor(cc);
                Color c = new Color(sc.R, sc.G, sc.B);

                var uartScreen = new MyUartScreen();
                uartService.SetScreen(uartScreen);

                tabPanel.AddTab(name, new Boundary {
                    Width = width,
                    MinHeight = heigth,
                    Content = new Background {
                        Color = c,
                        Content = uartScreen
                    }
                }, c);
                tabAvailable = true;
                Log.LogDebug($"Screen with {width}x{heigth} added -> {mainX}x{mainY}");
            }

            if (tabAvailable) {
                tabPanel.SelectTab(0);
            }

            var tabInput = new DockPanel {
                    Placement = DockPanel.DockedControlPlacement.Bottom,
                    DockedControl = new Boundary {
                        MaxHeight = 1,
                        MinHeight = 1,
                        Content = myInputBox
                    },
                    FillingControl = tabPanel
                };


            mainwin = new DockPanel {
                Placement = DockPanel.DockedControlPlacement.Bottom,
                DockedControl = new Boundary {
                    MaxHeight = 10,
                    MinHeight = 10,
                    Content = myLogPanel
                },
                FillingControl = tabInput
            };




        }

        private Thread? tuiThread;
        private IInputListener[]? input;

        public async Task StartAsync(CancellationToken cancellationToken) {
            Log.LogDebug("Program StartAsync called");

            ConsoleManager.Setup();
            ConsoleManager.Resize(new Size(mainX, mainY+16));
            ConsoleManager.Content = mainwin;

            input = new IInputListener[] {
                tabPanel,
                myInputLine,
                myInputBox
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
