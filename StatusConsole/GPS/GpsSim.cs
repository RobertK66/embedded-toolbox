using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StatusConsole.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.GPS {

    public class GpsSim {

        private IConfigurationSection thrConfig;
        private IOutputWrapper screen;
        private ILogger log;
        private ITtyService tty;

        public GpsSim(IConfigurationSection thrConfig, IOutputWrapper screen, ILogger log, ITtyService tty) {
            this.thrConfig = thrConfig;
            this.screen = screen;
            this.log = log;
            this.tty = tty;
        }

        internal void ProcessByte(int b) {
            // NO GPS Commands implemented yet
            screen.Write("received 0x" + b.ToString("X2") + " nothing implemented yet to react on GPS comannding!");
        }



    }       
}
