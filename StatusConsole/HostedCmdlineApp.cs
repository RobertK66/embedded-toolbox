using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace StatusConsole {
    public class HostedCmdlineApp : IHostedService {

        private readonly IUartService _myService;
        private Task guiInputHandler;

        // Constructor with IOC injection
        public HostedCmdlineApp(IUartService service) {
            _myService = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task StartAsync(CancellationToken cancellationToken) {
            Console.WriteLine("Hi starting 1");
            guiInputHandler = Task.Run(() => HandleConsoleInput());
            await _myService.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            Console.WriteLine("Hi stopping 1");
            await _myService.StopAsync(cancellationToken);
        }

        public void HandleConsoleInput() {
            String line = "";
            while( line != "quit") {
                line = Console.ReadLine();
                _myService.SendUart(line);
            }
        }
    }
}
