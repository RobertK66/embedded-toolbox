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

        override public void Read(SerialPort port) {
            // Avoid blocking the thread;
            // If nothing gets received, we sometimes have to check for the Continuation flag here.
            if (port.ReadTimeout == -1) {
                port.ReadTimeout = 500;
            }
            while(Continue) {
                try {
                    char ch = (char)port.ReadChar();
                    Log.LogInformation("Char {@mychar}", ch);
                    if (ch.ToString().Equals(port.NewLine)) {
                        Screen.WriteLine("");
                    } else {
                        Screen.Write(ch.ToString());
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
