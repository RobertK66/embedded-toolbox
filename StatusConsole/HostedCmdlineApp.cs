using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace StatusConsole {
    public class HostedCmdlineApp : IHostedService {

        private readonly IUartServices _myServices;
        private readonly Dictionary<string, IUartService> uarts;
        private Task guiInputHandler;
        private IUartService uartInFocus;

        // Constructor with IOC injection
        public HostedCmdlineApp(IUartServices service, IConfiguration conf) {
            _myServices = service ?? throw new ArgumentNullException(nameof(service));
            uarts = service.GetUartServices();
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            Console.WriteLine("Hi starting App");
            guiInputHandler = Task.Run(() => HandleConsoleInput());

            await uarts["DEBUG"].StartAsync(cancellationToken);
            await uarts["COM"].StartAsync(cancellationToken);

            uartInFocus = _myServices.GetCurrentService();
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            Console.WriteLine("Hi stopping App");
            await uarts["DEBUG"].StopAsync(cancellationToken);
            await uarts["COM"].StopAsync(cancellationToken);
        }

        public void HandleConsoleInput() {
            

            String line = "";
            while( line != "quit") {
                line = Console.ReadLine();
                if(!String.IsNullOrWhiteSpace(line)) {
                    uartInFocus?.SendUart(line);
                } else if(line == "\t") {
                    uartInFocus = _myServices.GetNextService();
                    Console.WriteLine("Switched to " + uartInFocus.GetInterfaceName());
                }
                    
            }
            Console.WriteLine("Input Handler closed!");
            // Trigger App close. The Exit works but
            // Exit code is not really passed on to OS from here!?
            Environment.ExitCode = -123;
            Environment.Exit(-234);
        }
    }
}
