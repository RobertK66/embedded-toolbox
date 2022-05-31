using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {
    public abstract class SerialPortBase: ITtyService {
        protected IConfigurationSection Config;
        private SerialPort Port;
        protected bool Continue;
        protected IOutputWrapper Screen;
        Task Receiver;
        protected ILogger? Log;
        private String? OnConnect;

        virtual public void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger? logger)  {
            Config = cs;
            OnConnect = cs.GetValue<String>("OnConnect", null);
            Log = logger;
        }

        string ITtyService.GetInterfaceName() {
            return Config.Key;
        }

        IConfigurationSection ITtyService.GetScreenConfig() {
            return Config.GetSection("Screen");
        }


        virtual public void SetScreen(IOutputWrapper scr) {
            Screen = scr;
        }


        public bool IsConnected() {
            return Continue;
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
                Log?.LogInformation("Uart Uart {@portname} connected. Using {@newline} as newline char.", Port.PortName, "0x" + Convert.ToByte(Port.NewLine[0]).ToString("X2"));

                    
                Continue = true;
                Receiver = Task.Run(() => Read(Port));
                SendUart(OnConnect);
            } catch (Exception ex) {
                Continue = false;
                Log?.LogError(ex, "Error starting '" + Config?.GetValue<String>("ComName") ?? "<null>->COM1" + "' !");
            }
            return Task.CompletedTask;
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            // terminate the reader Task.
            Continue = false;
            if (Receiver != null) {
                await Receiver;       // reader Task will be finished and execution "awaits it" and continues afterwards. (Without blocking any thread here)
                Port.Close();
                Log?.LogInformation("Uart {@portname} closed.",Port.PortName);
            }
        }


        public void SendUart(string line) {
            if (Continue) {
                try {
                    Port?.WriteLine(line);
                    Log?.LogDebug("Tx: {@line}", line);
                } catch (Exception ex) {
                    Log?.LogError("Fehler: " + ex.Message);
                }
            }
        }

        abstract public void Read(SerialPort port);

    }
}