using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    public class ObcEm2Uarts : IUartServices {

        private Dictionary<string, IUartService> services = new Dictionary<string, IUartService>();
        private List<string> keys = new List<string>();
        private IUartService currentService = null;

        public ObcEm2Uarts(IConfiguration conf) {
            var uartConfigs = conf?.GetSection("UARTS").GetChildren();
            foreach(var uc in uartConfigs) {
                var type = Type.GetType(uc.GetValue<String>("Impl")??"dummy");
                if(type != null) {
                    IUartService uartService = (IUartService)Activator.CreateInstance(type);
                    uartService.Initialize(uc);
                    
                    services.Add(uc.Key, uartService);
                    keys.Add(uc.Key);
                } else {
                    throw new ApplicationException("UART " + uc.Key + " Impl class not found!" );
                }
            }
            if(keys.Count > 0) {
                currentService = services[keys[0]];
            }
        }

        public Dictionary<string, IUartService> GetUartServices() {
            return services;
        }

        public IUartService GetCurrentService() {
            return currentService;
        }

        public IUartService GetNextService() {
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
