using ConsoleGUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.Controls {
    public class MyUartScreen : LogPanel, IOutputWrapper {
        public void Write(string v) {
            Add(v);
        }

        public void WriteData(byte[] buffer, int bytesRead) {
            Add(buffer.ToString());
        }

        public void WriteLine(string v) {
            Add(v);
        }

        public void WriteLine(string v, ConsoleColor red) {
            Add(v);
        }
    }
}
