using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {
    public class MyServiceCollection : IConfigurableServices {

        private static ILogger<MyServiceCollection> Log;

        private Dictionary<string, ITtyService> ttyServices = new Dictionary<string, ITtyService>();
        private List<string> keys = new List<string>();
        private ITtyService currentService = null;

        // This gets called by IOC Container and allows to read the Configuration (from appsettings.json)
        // Here we Instanciate all tty Services ("UARTS") configured
        public MyServiceCollection(IConfiguration conf, ILogger<MyServiceCollection> logger, ILoggerFactory logFactory) {
            Log = logger;
            Log.LogDebug("ServiceCollection constructor called");

            var uartConfigs = conf?.GetSection("UARTS").GetChildren();
            if (uartConfigs != null) {
                foreach (var uc in uartConfigs) {
                    // if not defined otherwise the 'Impl' class for a UART Connection is the SerialPortConnector
                    var type = Type.GetType(uc.GetValue<String>("Impl") ?? "StatusConsole.SerialPortConnector");
                    if (type != null) {
                        ITtyService ttyService = (ITtyService)Activator.CreateInstance(type);
                        ttyService?.Initialize(uc, conf, logFactory.CreateLogger(uc.Key));
                        ttyServices.Add(uc.Key, ttyService);
                        keys.Add(uc.Key);
                    } else {
                        throw new ApplicationException("UART " + uc.Key + " Impl class not found!");
                    }
                }
            }
            if(keys.Count > 0) {
                currentService = ttyServices[keys[0]];
            }
        }

        public Dictionary<string, ITtyService> GetTtyServices() {
            
            return ttyServices;
        }

        public ITtyService GetCurrentService() {
            return currentService;
        }

        public ITtyService GetNextService() {
            int curIdx = keys.FindIndex((s) => s == currentService.GetInterfaceName());
            if(curIdx != -1) {
                curIdx++;
                if(curIdx >= keys.Count) {
                    curIdx = 0;
                }
                currentService = ttyServices[keys[curIdx]];
            }
            return currentService;
        }

        public void SwitchCurrentService(int idx) {
            if (idx >= 0 && idx < keys.Count) {
                currentService = ttyServices[keys[idx]];
            }
        }

      
        IEnumerator<ITtyService> IEnumerable<ITtyService>.GetEnumerator() {
            return ttyServices.Values.GetEnumerator();
        }

        public IEnumerator GetEnumerator() {
            return ttyServices.Values.GetEnumerator();
        }

        public Task StartAsync(CancellationToken cancellationToken) {
            Log.LogDebug("ServiceCollection StartAsync called");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) {
            Log.LogDebug("ServiceCollection StopAsync called");
            return Task.CompletedTask;
        }
    }
}
