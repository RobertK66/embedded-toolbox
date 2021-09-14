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
                switch(uc.Key) {
                case "DEBUG":
                    IUartService debugCli = new UartCli();
                    debugCli.SetConfiguration(uc);
                    services.Add(uc.Key, debugCli);
                    keys.Add(uc.Key);
                    break;
                case "COM":
                    IUartService com = new UartCli();
                    com.SetConfiguration(uc);
                    services.Add(uc.Key, com);
                    keys.Add(uc.Key);
                    break;
                }
            }
            currentService = services[keys[0]];
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
