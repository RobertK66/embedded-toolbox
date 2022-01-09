using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    public class ServiceCollection : IConfigurableServices {

        private Dictionary<string, ITtyService> services = new Dictionary<string, ITtyService>();
        private List<string> keys = new List<string>();
        private ITtyService currentService = null;

        // This gets called by IOC Container and allows to read the Configuration (from appsettings.json)
        // Here we Instanciate all tty Services ("UARTS") configured
        public ServiceCollection(IConfiguration conf) {
            var uartConfigs = conf?.GetSection("UARTS").GetChildren();
            foreach(var uc in uartConfigs) {
                var type = Type.GetType(uc.GetValue<String>("Impl")??"dummy");
                if(type != null) {
                    ITtyService ttyService = (ITtyService)Activator.CreateInstance(type);
                    ttyService.Initialize(uc, conf);
                    
                    services.Add(uc.Key, ttyService);
                    keys.Add(uc.Key);
                } else {
                    throw new ApplicationException("UART " + uc.Key + " Impl class not found!" );
                }
            }
            if(keys.Count > 0) {
                currentService = services[keys[0]];
            }
        }

        public Dictionary<string, ITtyService> GetTtyServices() {
            return services;
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
                currentService = services[keys[curIdx]];
            }
            return currentService;
        }
        


    }
}
