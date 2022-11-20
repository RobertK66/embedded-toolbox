using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsoleApi {
    public interface ITtyService : IHostedService {
        void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger? logger);
        void SetScreen(IOutputWrapper scr);
        void SendUart(byte[] toSend, int count);

        //IConfigurationSection GetScreenConfig();
        string GetViewName();

        string GetInterfaceName();
        bool IsConnected();
        void ProcessCommand(String s);
    }
}
