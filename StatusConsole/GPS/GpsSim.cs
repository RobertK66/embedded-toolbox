using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatusConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace StatusConsole.GPS {

    public class GpsSim : ISerialProtocol {

        private IConfigurationSection thrConfig;
        private IOutputWrapper Screen;
        private ILogger Log;
        private ITtyService tty;


        Task Simulator;
        volatile int SendCount = 0;

        public GpsSim(IConfigurationSection thrConfig, IOutputWrapper screen, ILogger log, ITtyService tty) {
            this.thrConfig = thrConfig;
            this.Screen = screen;
            this.Log = log;
            this.tty = tty;
            Simulator = Task.Run(() => SendTimer());
        }

        public GpsSim(IConfigurationSection config) { }

        public void SetScreen(IConfigurationSection debugConfig, IOutputWrapper screen, Microsoft.Extensions.Logging.ILogger log, ITtyService tty) {
            this.thrConfig = debugConfig;
            this.Screen = screen;
            this.Log = log;
            this.tty = tty;
            Simulator = Task.Run(() => SendTimer());
        }

        public void ProcessByte(byte b) {
            // NO GPS Commands implemented yet
            Screen.Write("received 0x" + b.ToString("X2") + " nothing implemented yet to react on GPS comannding!");
        }


        public void SendTimer() {
            while (true) {
                try {
                    while (SendCount-- > 0) {
                        DateTime currentTime = DateTime.UtcNow;
                        String nmeaMessage = String.Format("$GPRMC,{0:00}{1:00}{2:00}.00,A,4201.1234,N,01212.1234,W,,,{3:00}{4:00}{5:00},,A*",
                            currentTime.Hour, currentTime.Minute, currentTime.Second,
                            currentTime.Day, currentTime.Month, currentTime.Year % 100);
                        nmeaMessage = AddCC(nmeaMessage) + "\r\n";
                        Screen.WriteLine("Sending '" + nmeaMessage + "'");
                        //tty.Port.Write(Encoding.ASCII.GetBytes(nmeaMessage), 0, nmeaMessage.Length);
                        tty.SendUart(Encoding.ASCII.GetBytes(nmeaMessage), nmeaMessage.Length);
                        Thread.Sleep(970);
                    }
                    Thread.Sleep(100);
                } catch (Exception ex) {
                    Screen.WriteLine("Send Timer  terminated with Exception: " + ex.Message, ConsoleColor.Red);
                    Screen.WriteLine("Try to reconnect with <ESC>.");           // TODO: Esc->reconnect is a feature of (G)TUI. Should be signaled from higher level of application !!!????
                    Log?.LogError(new EventId(2, "Rx"), ex, "Error: Closing reader!");
                    //Continue = false;
                }
            }
            //Screen.WriteLine("Send Timer  terminated");

        }


        private string AddCC(string nmeaMessage) {
            int Checksum = 0;
            foreach (char c in nmeaMessage) {
                if (c != '$' && c != '*') {
                    Checksum = Checksum ^ c;
                }
            }
            return nmeaMessage + Checksum.ToString("X2");
        }

        public void ProcessCommand(string cmd) {
            if (cmd == "1") {
                SendCount = 6;
            } else {
                Screen.WriteLine("nothing sent (use '1' only)");
            }
        }
    }       
}
