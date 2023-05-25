using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StatusConsole.L3;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StatusConsoleApi;

namespace StatusConsole {
   
    public class RemoteNextionDisplay2 : ITtyService {
        private IConfigurationSection Config;
        private SerialPort Port;
        private bool Continue;
        IOutputWrapper Screen;
        Task Receiver;

        private NextionL3 Nextion = new NextionL3();

        public void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger logger)  {
            Config = cs;
            Nextion.NextionEventReceived += Nextion_NextionEventReceived;
        }

        private void Nextion_NextionEventReceived(object arg1, NextionEventArgs arg2) {
            Screen.WriteLine(arg2.Message??"???");
        }

        string ITtyService.GetInterfaceName() {
            return Config.Key;
        }

        //IConfigurationSection ITtyService.GetScreenConfig() {
        //    return Config.GetSection("Screen");
        //}

        public Task StartAsync(CancellationToken cancellationToken) {
            try {
                Port = new SerialPort();
                Port.PortName = Config?.GetValue<String>("ComName") ?? "COM1";
                Port.BaudRate = Config?.GetValue<int?>("Baud") ?? 9600;
                Port.Parity = Parity.None;
                Port.DataBits = 8;
                Port.StopBits = StopBits.One;
                Port.Handshake = Handshake.None;
                Port.Open();
                //Port.NewLine = ''; Config?.GetValue<String>("NewLine") ?? "\r";
                //_serialPort.ReadTimeout = 10;
                Screen.WriteLine("Uart " + Port.PortName + " connected");
                Continue = true;
                Receiver = Task.Run(() => Read());
            } catch (Exception ex) {
                Continue = false;
                Screen.WriteLine("Error starting '" + Config?.GetValue<String>("ComName") ?? "<null>->COM1" + "' !", ConsoleColor.Red);
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
            byte[] data = new byte[500];
            int idx = 0;
            while (Continue) {
                try {
                    int x = Port.Read(data, idx, 100);
                    Nextion.ProcessReceivedData(data, x);
                    //char ch = (char)Port.ReadChar();
                    //if (ch.ToString().Equals(Port.NewLine)) {
                    //    Screen.WriteLine("");
                    //} else {
                    //    Screen.Write(ch.ToString());
                    //}
                } catch (TimeoutException) { }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken) {
            // terminate the reader Task.
            Continue = false;
            if (Receiver != null) {
                await Receiver;       // reader Task will be finished and execution "awaits it" and continues afterwards. (Without blocking any thread here)
                Port.Close();
                Screen.WriteLine("Uart " + Port.PortName + " closed.");
            }
        }

        public bool IsConnected() {
            return Continue;
        }

        public void SetScreen(IOutputWrapper scr) {
            Screen = scr;
        }

        public void SendUart(byte[] toSend, int len) {
            throw new NotImplementedException();
        }

        public void ProcessCommand(string s) {
            throw new NotImplementedException();
        }

        public string GetViewName() {
            throw new NotImplementedException();
        }
    }
}
