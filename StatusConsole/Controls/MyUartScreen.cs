using ConsoleGUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.Controls {
    public class MyUartScreen : LogPanel, IOutputWrapper {

        String Line = "";

        public void Write(string v) {
            if (v.Contains("\r")) {
                Line += v.Substring(0, v.IndexOf("\r"));
                Add(Line);
                Line = v.Substring(v.IndexOf("\r") + 1);
            } else {
                Line += v;
            }
        }

        public void WriteData(byte[] buffer, int bytesRead) {
            Add(buffer.ToString());
        }

        public void WriteLine(string v) {
            if (!string.IsNullOrEmpty(Line)) {
                Add(Line + v);
                Line = "";
            } else {
                Add(v);
            }
        }

        public void WriteLine(string v, ConsoleColor red) {
            Add(v);
        }

    }
}
