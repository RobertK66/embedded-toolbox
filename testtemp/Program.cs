using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace testtemp {

    class Program {
        public async static Task<int> Main(string[] args) {
            var host = CreateHostBuilder(args);
            await host.RunConsoleAsync();
            // This lines are not reached if Environment.Exit() is used somewhere in Services.....
            Console.WriteLine("Exit Code in Main[]:" + Environment.ExitCode);
            return Environment.ExitCode;
        }

        private static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder()
                       .ConfigureServices(services => {
                           services.AddHostedService<HostedService>();
                       });
        }

    }
}