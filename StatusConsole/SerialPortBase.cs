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
        protected ILogger Log;

        virtual public void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger logger)  {
            Config = cs;
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
                Screen.WriteLine("Uart " + Port.PortName + " connected");
                Continue = true;
                Receiver = Task.Run(() => Read(Port));
            } catch (Exception ex) {
                Continue = false;
                Screen.WriteLine("Error starting '" + Config?.GetValue<String>("ComName") ?? "<null>->COM1" + "' !", ConsoleColor.Red);
                Screen.WriteLine(ex.Message, ConsoleColor.Red);
            }
            return Task.CompletedTask;
        }

        async Task IHostedService.StopAsync(CancellationToken cancellationToken) {
            // terminate the reader Task.
            Continue = false;
            if (Receiver != null) {
                await Receiver;       // reader Task will be finished and execution "awaits it" and continues afterwards. (Without blocking any thread here)
                Port.Close();
                Screen.WriteLine("Uart " + Port.PortName + " closed.");
            }
        }


        void ITtyService.SendUart(string line) {
            if (Continue) {
                try {
                    Port?.WriteLine(line);
                } catch (Exception ex) {
                    Screen.WriteLine("Fehler: " + ex.Message);
                }
            }
        }

        abstract public void Read(SerialPort port);

    }
}