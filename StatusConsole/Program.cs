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
using ScreenLib;

namespace StatusConsole {

    class Program
    {
        public async static Task<int> Main(string[] args) { 
            var host = CreateHostBuilder(args);
            await host.RunConsoleAsync();
            // This lines are not reached if Environment.Exit() is used somewhere in Services.....
            Console.WriteLine("Exit Code in Main[]:" + Environment.ExitCode);
            return Environment.ExitCode;      
        }

        private static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder()
                        .ConfigureAppConfiguration((hbc,cb) => {
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
                        })
                       .ConfigureServices(services => {
                           services.AddHostedService<HostedCmdlineApp>();
                           services.AddTransient<IUartServices, ObcEm2Uarts>();
                           
                       });
        }


        //private Screen sca;
        //private Screen scb;
        //private Screen scc;

        //public void Run() {
        //    MainScreen main = new MainScreen(110,30);
        //    sca = main.AddScreen(0,0, new Screen(50, 10));
        //    scb = main.AddScreen(50,0, new Screen(50, 10, ConsoleColor.Yellow, ConsoleColor.Black));
        //    scc = main.AddScreen(0,14, new Screen(100, 14, ConsoleColor.DarkBlue, ConsoleColor.White));
        //    scb.VertType = VerticalType.WRAP_AROUND;
        //    scb.HoriType = HorizontalType.WRAP;
        //    main.Clear(true);

        //    sca.WriteLine("Hallo Screen A");
        //    scb.WriteLine("Hi to Screen B");
        //    scc.WriteLine("---- Trööööt -------");
        //    sca.WriteLine("---------------");
        //    scb.WriteLine("**************");
        //    scc.WriteLine("----------------------------------------> Schirm C hier");

        //    main.LineEntered += LineHandler;

        //    main.RunLine("quit");

        //}

        //public void LineHandler(object sender, LineEnteredArgs e) {
        //    scb.WriteLine(e.Cmd);
        //    scb.WritePosition(40, 3, e.Cmd.ToUpper(), 7);
        //}
      
    }

}
