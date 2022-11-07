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
    public class ThrusterSimulator: SerialPortBase {

        Thruster.ThrusterSim sim;
        IConfigurationSection thrConfig;

        override public void Initialize(IConfigurationSection cs, IConfiguration rootConfig, ILogger logger) {
            base.Initialize(cs, rootConfig, logger);

            // The value of "ImplsConfigSection" has the name of another Config Section, which we have to search for in the whole appsettings config....
            // TODO: Refactor the main app setting, appstart and hierachies of classes and IOC and so on......
            String configName = Config?.GetValue<String>("ImplConfigSection");
            thrConfig = rootConfig?.GetSection(configName);
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
                    sim.ProcessByte(b);
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
            sim = new Thruster.ThrusterSim(thrConfig, Screen, Log, this);
        }

      
    }
}
