using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ScreenLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    public interface ITtyService : IHostedService {
        void Initialize(IConfigurationSection cs);
        void SetScreen(IConOutput scr);
        void SendUart(string line);

        IConfigurationSection GetScreenConfig();

        string GetInterfaceName();
        bool IsConnected();
    }
}
