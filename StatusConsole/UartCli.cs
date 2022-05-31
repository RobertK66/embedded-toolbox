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
    public class UartCli : SerialPortBase {

        private string debugLine ="";

        override public void Read(SerialPort port) {
            // Avoid blocking the thread;
            // If nothing gets received, we sometimes have to check for the Continuation flag here.
            if (port.ReadTimeout == -1) {
                port.ReadTimeout = 500;
            }
            while(Continue) {
                try {
                    char ch = (char)port.ReadChar();
                    Log?.LogTrace("Rx: {@mycharHex} '{@mychar}'", "0x"+Convert.ToByte(ch).ToString("X2"), (ch=='\n')?' ':ch);
                    if (ch.ToString().Equals(port.NewLine)) {
                        Screen.WriteLine("");
                        Log?.LogDebug("Rx: " + debugLine);
                        debugLine = "";
                    } else {
                        Screen.Write(ch.ToString());
                        debugLine += ch.ToString();
                    }
                } catch (TimeoutException) {
                } catch (Exception ex) {
                    Screen.WriteLine("Exception im reader: " + ex.Message);
                    Continue = false;
;                }
            }
        }
      
    }
}
