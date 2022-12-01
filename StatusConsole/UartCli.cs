using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StatusConsoleApi;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {
//    public class UartCli : SerialPortBase {

//        private string debugLine ="";

//        override public void Read(SerialPort port) {
//            // Avoid blocking the thread;
//            // If nothing gets received, we sometimes have to check for the Continuation flag here.
//            if (port.ReadTimeout == -1) {
//                port.ReadTimeout = 500;
//            }
//            while(Continue) {
//                try {
//                    char ch = (char)port.ReadChar();
//                    Log?.LogTrace(new EventId(2, "Rx"), "{@mycharHex} '{@mychar}'", "0x"+Convert.ToByte(ch).ToString("X2"), (ch=='\n')?' ':ch);
//                    if (ch.ToString().Equals(port.NewLine)) {
//                        Screen.WriteLine("");
//                        Log?.LogDebug(new EventId(2, "Rx"), debugLine);
//                        debugLine = "";
//                    } else {
//                        Screen.Write(ch.ToString());
//                        debugLine += ch.ToString();
//                    }
//                } catch (TimeoutException) {
//                    // Do nothing. This is only here to get while condition checked.
//                } catch (Exception ex) {
//                    Screen.WriteLine("Reader terminated with Exception: " + ex.Message, ConsoleColor.Red);
//                    Screen.WriteLine("Try to reconnect with <ESC>.");           // TODO: Esc->reconnect is a feature of (G)TUI. Should be signaled from higher level of application !!!????
//                    Log?.LogError(new EventId(2, "Rx"),ex, "Error: Closing reader!");
//                    Continue = false;
//;                }
//            }
//        }
      
//    }
}
