using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {
    public class GpsSimulator: SerialPortBase {

        //GPS.GpsSim sim;
        IConfigurationSection thrConfig;
        Task Simulator;
        volatile int SendCount = 0;

        override public void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger logger) {
            base.Initialize(cs, rootConfig, logger);

            // The value of "ImplsConfigSection" has the name of another Config Section, which we have to search for in the whole appsettings config....
            // TODO: Refactor the main app setting, appstart and hierachies of classes and IOC and so on......
            String configName = Config?.GetValue<String>("ImplConfigSection");
            thrConfig = rootConfig?.GetSection(configName);
            //Simulator = Task.Run(() => SendTimer());
        }

        private void SendTimer() {
            while (true) {
                try {
                    while (SendCount-- > 0) {
                        DateTime currentTime = DateTime.UtcNow;
                        String nmeaMessage = String.Format("$GPRMC,{0:00}{1:00}{2:00}.00,A,4201.1234,N,01212.1234,W,,,{3:00}{4:00}{5:00},,A*",
                            currentTime.Hour, currentTime.Minute, currentTime.Second,
                            currentTime.Day, currentTime.Month, currentTime.Year % 100);
                        nmeaMessage = AddCC(nmeaMessage) + "\r\n";
                        Screen.WriteLine("Sending '" + nmeaMessage + "'");
                        Port.Write(Encoding.ASCII.GetBytes(nmeaMessage), 0, nmeaMessage.Length);
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

        override public void Read(SerialPort port) {
            // Avoid blocking the thread;
            // If nothing gets received, we sometimes have to check for the Continuation flag here.
            if (port.ReadTimeout == -1) {
                port.ReadTimeout = 500;
            }
            while(Continue) {
                try {
                    int b = port.ReadByte();
                    Log?.LogTrace("Rx: {@mycharHex} '{@mychar}'", "0x" + b.ToString("X2"), (b == '\n') ? ' ' : b);
                    Screen.Write("received 0x" + b.ToString("X2") + " nothing implemented yet to react on GPS comannding!");
                } catch(TimeoutException) {
                    // Do nothing. This is only here to get while condition checked.
                } catch (Exception ex) {
                    Screen.WriteLine("Reader terminated with Exception: " + ex.Message, ConsoleColor.Red);
                    Screen.WriteLine("Try to reconnect with <ESC>.");           // TODO: Esc->reconnect is a feature of (G)TUI. Should be signaled from higher level of application !!!????
                    Log?.LogError(new EventId(2, "Rx"), ex, "Error: Closing reader!");
                    Continue = false;
                }
        }
        }


        override public void SetScreen(IOutputWrapper scr) {
            Screen = scr;
            Continue = true;
            Simulator = Task.Run(() => SendTimer());
            //sim = new GPS.GpsSim(thrConfig, Screen, Log, this);
        }

        public override byte[] ProcessCommand(string s) {
            String nmeaMessage = "";
            if (s == "1") {
                SendCount = 6;
            } else {
                Screen.WriteLine("nothing sent (use '1' only)");
            }
            return Encoding.UTF8.GetBytes(nmeaMessage);
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
    }
}
