using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.Logger {
    public class ConGuiLoggerConfiguration {

        public LogPanel? LogPanel { get; set; } = null;
        //public int EventId { get; set; }

        public Dictionary<string, LogLevel> LogLevel { get; set; } = new() {
            ["Default"] = Microsoft.Extensions.Logging.LogLevel.Error
        };
    }
}
