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
        private static String myLock = "55";
        private static LogPanel myLogPanel = new(myLock, ConsoleColor.Blue.GetGuiColor());

        public async static Task<int> Main(string[] args) {
            // This is a Programm runing in a hosted enviroment. So the static Main() configures everything and let the host start the Application.
            var host =  Host.CreateDefaultBuilder()
                       .ConfigureAppConfiguration((hbc, configBuilder) => {
                           // Commandline pars with "--<anyname> <value>" automatically appear as "anyname" pars in the flattened config.
                           // to also use "-<anyname> <value>" you have to provide the specific mappings here
                           // The example mypar now works with either -mypar ode --mypar !!!
                           Dictionary<string, string> mappings = new Dictionary<string, string>() {
                                { "-mypar", "mypar" } };
                           configBuilder.AddCommandLine(args, mappings);
                       })
                       .ConfigureLogging((hbc, loggingBuilder) => {
                           loggingBuilder.ClearProviders();             // This avoids default logging output to console.
                           loggingBuilder.AddConGuiLogger((con) => {    // This adds our LogPanel as possible target (configure in appsettings.json)
                               con.LogPanel = myLogPanel;
                           });
                           loggingBuilder.AddDebug();                   // This gives Logging in the Debug Console of VS. (configure in appsettings.json)
                           
                           // This giving possible file logger implementation (serilog). Note Debug and ConGui works without serilog dependency!
                           loggingBuilder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(hbc.Configuration).CreateLogger());
                       })
                       .ConfigureServices(serviceCollection => {
                           serviceCollection.AddTransient<IConfigurableServices, MyServiceCollection>();
                           // Assign this Program class as Service -> so we get instanciated and called by Container for startup.....
                           serviceCollection.AddHostedService<Program>();
                       });

            await host.RunConsoleAsync();

            // Switch back colors to leave a usable Console/Cmd window
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            return Environment.ExitCode;      

        }



        // Program Instance part
        private ILogger<Program> _Log;
        private readonly IConfigurableServices serviceCollection;
        private Thread? tuiThread;

        // T(G)UI
        private List<IInputListener> inputListeners = new List<IInputListener>();
        private IControl mainwin = null;
        private TabPanel tabPanel = new();
        private MyInputController myInputController = new MyInputController();
        private MyFunctionController? myFunctionController = null;
        //private int mainX = 0;
        private int mainY = 0;

        // As a 'hosted service', constructor of Program gets called by host with DI-resolved logger and serviceCollection
        // We do generate and 'wire up' all UI components for each (Uart)service here.
        public Program(IConfiguration conf, ILogger<Program> logger , IConfigurableServices services) {
            _Log = logger;
            _Log.LogDebug("Program() Constructor called.");

            serviceCollection = services;
            myFunctionController = new MyFunctionController(services);
           
            bool tabAvailable = false;
            foreach (var uartService in services) {
                
                string name = uartService.GetInterfaceName();
                String screenName = uartService.GetViewName();

                IConfigurationSection css = conf.GetSection("SCREENS");
                IConfigurationSection cs = css.GetSection(screenName);

                //int width = cs.GetValue("Width", 80);
                int heigth = cs.GetValue("Height", 10);
                //if (width > mainX) {
                //    mainX = width;
                //}
                if (heigth > mainY) {
                    mainY = heigth;
                }
                
                ConsoleColor cc = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), cs.GetValue<String>("Background",null)??"Black");
                Color backgroundColor = cc.GetGuiColor();
                cc = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), cs.GetValue<String>("Text",null)??"White");
                Color textColor = cc.GetGuiColor();

                String time = cs.GetValue("Time", "None")??"None";
                Color? timeColor = null;
                if (time != "None") {
                    cc = (ConsoleColor)Enum.Parse(typeof(ConsoleColor), time);
                    timeColor = cc.GetGuiColor();
                }
                var uartScreen = new MyUartScreen(myLock, backgroundColor, textColor, timeColor);
                inputListeners.Add(uartScreen);
                TextBox textBox = new TextBox();
                uartService.SetScreen(uartScreen);

                tabPanel.AddTab(name, new Boundary {
                    //Width = width,
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
                myInputController.AddCommandLine(textBox, uartService.ProcessCommand);
                myFunctionController.AddUartScreen(name, uartScreen);
                _Log.LogTrace($"Screen with widthx{heigth} added -> mainXx{mainY}");
            }

            if (tabAvailable) {
                tabPanel.TabSwitched += TabPanel_TabSwitched;
                tabPanel.SelectTab(0);
            }

            mainwin = new DockPanel {
                Placement = DockPanel.DockedControlPlacement.Bottom,
                DockedControl = new Boundary {
                    MaxHeight = 9,
                    MinHeight = 9,
                    Content = myLogPanel
                },
                FillingControl = tabPanel
            };
        }

        private void TabPanel_TabSwitched(object sender, TabSwitchedArgs e) {
            myInputController.SetActive(e.selectedIdx);
            serviceCollection.SwitchCurrentService(e.selectedIdx);
        }

     
        // This gets called by host - after Constructor.
        public async Task StartAsync(CancellationToken cancellationToken) {
            _Log.LogDebug("Program StartAsync called");

            ConsoleManager.Console = new SimplifiedConsole();
            ConsoleManager.Setup();
            //ConsoleManager.Resize(new Size(mainX, mainY+16));
            ConsoleManager.Content = mainwin;

         
            inputListeners.Insert(0, tabPanel);
            inputListeners.Insert(1, myInputController);
            inputListeners.Insert(2, myFunctionController);

            // We start the UI thread which takes care of the Console Input.
            tuiThread = new Thread(new ThreadStart(TuiThread));
            tuiThread.Start();

            // The single uartService Objects are POCOs (generated from Config and not DI!).
            // Here we add them manually to the 'StartAsync' cadence of the host.
            foreach (var uartService in serviceCollection) {
                await uartService.StartAsync(cancellationToken);
            }
        }

        // When host wants to stop we do wait for all serviceCollection to stop here.
        public async Task StopAsync(CancellationToken cancellationToken) {
            _Log.LogDebug("Program StopAsync called");
            // Stop all UART Coms
            await serviceCollection.ForEachAsync(uart => uart.StopAsync(cancellationToken));
            tuiThread?.Interrupt();
            while (tuiThread?.IsAlive??false) {
                Thread.Sleep(50);
            }
            // Clear all content and switch color of Console for usage after this Program ....
            ConsoleManager.Content = new Style() { Background = Color.Black, Foreground = Color.White };
            //Console.WriteLine("");
            //Console.WriteLine("Program StopAsync finished");
        }

        private void TuiThread() {
            try {
                _Log.LogDebug(new EventId(1, "TUI"), "TUI Thread started");
                while (true) {
                        ConsoleManager.AdjustBufferSize();  // Resize for Windows!
                    ConsoleManager.ReadInput(inputListeners);
                    Thread.Sleep(50);
                }
            } catch (ThreadInterruptedException) {
                _Log.LogDebug("TUI Thread canceled by InterruptException");
            }
        }
    }

}
