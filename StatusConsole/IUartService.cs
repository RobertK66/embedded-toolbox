using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    public interface IUartService : IHostedService {
        void SendUart(string line);
        void SetConfiguration(IConfigurationSection cs);
        string GetInterfaceName();
    }
}
