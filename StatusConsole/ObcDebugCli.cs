using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {
    public class ObcDebugCli : ITtyService {
        private IConfigurationSection Config;
        private SerialPort  Port;
        private bool Continue;
        IOutputWrapper Screen;
        Task Receiver;

        OBC.ObcDebug debug;
        IConfigurationSection debugConfig;

        public void Initialize(IConfigurationSection cs, IConfiguration rootConfig ) {
            Config = cs;    
            String configName = Config?.GetValue<String>("ImplConfigSection");
            // The value of "ImplsConfigSection" has the name of another Config Section, which we have to search for in the whole appsettings config....
            // TODO: Refactor the main app setting, appstart and hierachies of classes and IOC and so on......
            debugConfig = rootConfig?.GetSection(configName);
        }

        string ITtyService.GetInterfaceName() {
            return Config.Key;
        }

        IConfigurationSection ITtyService.GetScreenConfig() {
            return Config.GetSection("Screen");
        }
        
        Task IHostedService.StartAsync(CancellationToken cancellationToken) {
            try {
                Port = new SerialPort();
                Port.PortName = Config?.GetValue<String>("ComName") ?? "COM1";
                Port.BaudRate = Config?.GetValue<int?>("Baud") ?? 9600;
                Port.Parity = Parity.None;
                Port.DataBits = 8;
                Port.StopBits = StopBits.One;
                Port.Handshake = Handshake.None;
                Port.Open();
                Port.NewLine = Config?.GetValue<String>("NewLine") ?? "\r";
                //_serialPort.ReadTimeout = 10;
                Screen.WriteLine("Uart " + Port.PortName + " connected");
                Continue = true;
                Receiver = Task.Run(() => Read());
            } catch (Exception ex) {
                Continue = false;
                Screen.WriteLine("Error starting '" + Config?.GetValue<String>("ComName")??"<null>->COM1" + "' !", ConsoleColor.Red);
                Screen.WriteLine(ex.Message, ConsoleColor.Red);
            }
            return Task.CompletedTask;
        }

        public void Read() {
            // Avoid blocking the thread;
            // If nothing gets received, we sometimes have to check for the Continuation flag here.
            if (Port.ReadTimeout == -1) {
                Port.ReadTimeout = 500;
            }
            while(Continue) {
                try {
                    int b = Port.ReadByte();
                    debug.ProcessByte(b);
                } catch(TimeoutException) { }
            }
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            // terminate the reader Task.
            Continue = false;
            if(Receiver != null) {
                await Receiver;       // reader Task will be finished and execution "awaits it" and continues afterwards. (Without blocking any thread here)
                Port.Close();
                Screen.WriteLine("Uart " + Port.PortName + " closed.");
            }
        }

        void ITtyService.SendUart(string line) {
            if(Continue) {
                Port?.WriteLine(line);
            }
        }

        public bool IsConnected() {
            return Continue;
        }

        public void SetScreen(IOutputWrapper scr) {
            Screen = scr;
            debug = new OBC.ObcDebug(debugConfig, Screen);
        }

      
    }
}
