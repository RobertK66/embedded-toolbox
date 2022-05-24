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
using StatusConsole.Logger;
using Serilog;

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
                       .ConfigureLogging((hostContext, cl) => {
                           cl.ClearProviders();             // This avoids default logging output to console.
                           cl.AddConGuiLogger((con) => {    // This adds our LogPanel as possible target (configure in appsettings.json)
                               con.LogPanel = myLogPanel;
                           });
                           cl.AddDebug();                   // This gives Logging in the Debug Console of VS. (configure in appsettings.json)
                                                            // This giving possible file logger implementation (serilog). Note Debug and ConGui works without serilog dependency!
                           cl.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(hostContext.Configuration).CreateLogger());
                       })
                       .ConfigureServices(services => {
                           services.AddTransient<IConfigurableServices, MyServiceCollection>();
                           services.AddHostedService<Program>();
                       });

            await host.RunConsoleAsync();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            return Environment.ExitCode;      
        }

        // Program Instance part
        private ILogger<Program> _Log;
        private readonly IConfigurableServices uartServices;

        // T(G)UI
        private List<IInputListener> input = new List<IInputListener>();
        private static String myLock = "55";
        private static LogPanel myLogPanel = new(myLock, ConsoleColor.Blue.GetGuiColor());
        private IControl mainwin = null;
        private TabPanel tabPanel = new();
        private MyInputController? myInputController = null;
        private MyFunctionController? myFunctionController = null;
        private int mainX = 0;
        private int mainY = 0;


        public Program(IConfiguration conf, ILogger<Program> logger , IConfigurableServices services) {
            _Log = logger;
            _Log.LogDebug("Program() Constructor called.");

            uartServices = services;
            myInputController = new MyInputController();
            myFunctionController = new MyFunctionController(services);
      
           
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
                Color backgroundColor = cc.GetGuiColor();
                cc = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), cs.GetValue("Text", "Whilte"));
                Color textColor = cc.GetGuiColor();

                string time = cs.GetValue("Time", "None");
                Color? timeColor = null;
                if (time != "None") {
                    cc = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), time);
                    timeColor = cc.GetGuiColor();
                }


                var uartScreen = new MyUartScreen(myLock, backgroundColor, textColor, timeColor);
                input.Add(uartScreen);
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
                                MaxHeight = 3,
                                MinHeight = 3,
                                Content = new ForceForeground(textColor) { Content = new Border() { Content = textBox, BorderStyle = BorderStyle.Single } }
                            },
                            FillingControl = uartScreen
                        }
                    }
                }, backgroundColor, new Color(128, 128, 128), textColor);
                tabAvailable = true;
                myInputController.AddCommandLine(textBox, CommandCallback);
                myFunctionController.AddUartScreen(name, uartScreen);
                _Log.LogDebug($"Screen with {width}x{heigth} added -> {mainX}x{mainY}");
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
     

        public async Task StartAsync(CancellationToken cancellationToken) {
            _Log.LogDebug("Program StartAsync called");

            ConsoleManager.Console = new SimplifiedConsole();
            ConsoleManager.Setup();
            ConsoleManager.Resize(new Size(mainX, mainY+16));
            ConsoleManager.Content = mainwin;

            //input = new IInputListener[] {
            //    tabPanel,
            //    myInputController,
            //    myFunctionController
            //  };
            input.Insert(0, tabPanel);
            input.Insert(1, myInputController);
            input.Insert(2, myFunctionController);

            tuiThread = new Thread(new ThreadStart(TuiThread));
            tuiThread.Start();

            foreach (var uartService in uartServices) {
                await uartService.StartAsync(cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            _Log.LogDebug("Program StopAsync called");
            // Stop all UART Coms
            await uartServices.ForEachAsync(uart => uart.StopAsync(cancellationToken));
            // Clear all content and switch color of Console for usage after this Program ....
            ConsoleManager.Content = new Style() { Background = Color.Black, Foreground = Color.White };
        }

        private void TuiThread() {
            try {
                _Log.LogDebug("TUI Thread started");
                while (true) {
                    ConsoleManager.ReadInput(input);
                    Thread.Sleep(20);
                }
            } catch (ThreadInterruptedException) {
                _Log.LogDebug("TUI Thread canceled by InterruptException");
            }
        }

        private void CommandCallback(string command) {
            _Log.LogDebug("Command " + command);
            var s = uartServices.GetCurrentService();
            s.SendUart(command);
        }

    }

}
