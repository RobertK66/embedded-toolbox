using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatusConsoleApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace StatusConsolePlugins.GPS {

    public class GpsSim : ISerialProtocol {

        private IConfigurationSection? thrConfig;
        private IOutputWrapper? Screen;
        private ILogger? Log;
        private ITtyService? tty;

        Task Simulator;
        volatile int SendCount = 0;
     
        // This constructor is called by the 'plugin System' when somb. is configured to use this 'protocol Class'
        public GpsSim(IConfigurationSection config) {
            // Start our background sim thread
            Simulator = Task.Run(() => SendTimer());
        }

        public void SetScreen(IOutputWrapper screen, Microsoft.Extensions.Logging.ILogger log, ITtyService tty) {
            this.Screen = screen;
            this.Log = log;
            this.tty = tty;
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
                        Screen?.WriteLine("Sending '" + nmeaMessage + "'");
                        //tty.Port.Write(Encoding.ASCII.GetBytes(nmeaMessage), 0, nmeaMessage.Length);
                        tty?.SendUart(Encoding.ASCII.GetBytes(nmeaMessage), nmeaMessage.Length);
                        Thread.Sleep(970);  // With this 970ms we get the OBC to sync because it accepts the 'seconds' interval as 'precise enough'. (1000-> to much time in between!)
                    }
                    Thread.Sleep(100);
                } catch (Exception ex) {
                    Screen?.WriteLine("Send Timer  terminated with Exception: " + ex.Message, ConsoleColor.Red);
                    Screen?.WriteLine("Try to reconnect with <ESC>.");           // TODO: Esc->reconnect is a feature of (G)TUI. Should be signaled from higher level of application !!!????
                    Log?.LogError(new EventId(2, "Rx"), ex, "Error: Closing timer!");
                    //Continue = false;
                }
            }
            //Screen?.WriteLine("Send Timer  terminated");

        }


        // UART Incomming Byte protocol
        public void ProcessByte(byte b) {
            // NO GPS Commands implemented yet
            Screen?.Write("received 0x" + b.ToString("X2") + " nothing implemented yet to react on GPS comannding!");
        }


        //G/TUI inputs from Users Command Line
        public void ProcessUserInput(string cmd) {
            if (cmd == "1") {
                SendCount = 6;  // make 6 GPRMC commands with 1 second interval to trigger a time sync in OBC.
            } else {
                Screen?.WriteLine("nothing sent (use '1' only)");  // TODO: helop and commands structure for simulators...
            }
        }


        // Caclulate and append NMEA checksum to message string
        private string AddCC(string nmeaMessage) {
            int Checksum = 0;
            foreach (char c in nmeaMessage) {
                if (c != '$' && c != '*') {
                    Checksum = Checksum ^ c;
                }
            }
            return nmeaMessage + Checksum.ToString("X2");
        }

    }       
}
