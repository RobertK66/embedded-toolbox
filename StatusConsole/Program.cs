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

namespace StatusConsole {

    public class Program : IHostedService
    {
        private static IServiceCollection ProgServices;

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
                           services.AddHostedService<Program>();
                           //services.AddTransient<ITtyService>();
                           ProgServices = services;     // Lets do the Detailed Service configuration from Program Instance later in Constructor there.
                       });


            await host.RunConsoleAsync();

            // This lines are not reached if Environment.Exit() is used somewhere in Services.....
            Console.WriteLine("Exit Code in Main[]:" + Environment.ExitCode);
            return Environment.ExitCode;      
        }


        // Program Instance part
        private readonly ILogger<Program> Log;

        public Program(IConfiguration conf, ILogger<Program> logger) {
            Log = logger;
            Log.LogDebug("Program() Constructor called.");

            //foreach (var service in ProgServices) {
            //    Log.LogDebug(service.Lifetime +  " " + service.ServiceType + " " + service.ImplementationType);         }
            var uartConfigs = conf?.GetSection("UARTS").GetChildren();

            foreach (var uc in uartConfigs) {
                var type = Type.GetType(uc.GetValue<String>("Impl") ?? "dummy");
                ITtyService instance = (ITtyService)ProgServices.AddTransient(type);
                //    ITtyService ttyService = (ITtyService)Activator.CreateInstance(type);
                instance.Initialize(uc, conf);

                //    ProgServices.AddSingleton(ttyService);

                //    //services.Add(uc.Key, ttyService);
                //    //keys.Add(uc.Key);
                //} else {
                //    throw new ApplicationException("UART " + uc.Key + " Impl class not found!");
                //}
            }
            //if (keys.Count > 0) {
            //currentService = services[keys[0]];
        //}

        }

        public Task StartAsync(CancellationToken cancellationToken) {
            Log.LogDebug("Program StartAsync called");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Log.LogDebug("Program StopAsync called");
            return Task.CompletedTask;
        }
    }

}
