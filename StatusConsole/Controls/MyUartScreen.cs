using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using StatusConsoleApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole.Controls {
    public class MyUartScreen : LogPanel, IOutputWrapper {

        String Line = "";
        private Color backgroundColor;
        private Color textColor;


        public MyUartScreen(object monitorObject, Color backgroundCol, Color textCol, Color? timeColor) : base(monitorObject, timeColor) {
            this.backgroundColor = backgroundCol;
            this.textColor = textCol;
        }

        public void Write(string v) {
            Monitor.Enter(monitorObject);
            if (v.Contains("\r")) {
                Line += v.Substring(0, v.IndexOf("\r"));
                Add(Line, textColor);
                Line = v.Substring(v.IndexOf("\r") + 1);
            } else {
                Line += v;
            }
            Monitor.Exit(monitorObject);
        }

        public void WriteData(byte[] buffer, int bytesRead) {
            Monitor.Enter(monitorObject);
            String dataline = "";
            foreach (byte b in buffer ) {
                dataline += " "+ b.ToString("X2");
            }
            Add(dataline, textColor);
            Monitor.Exit(monitorObject);
        }

        public void WriteLine(string v) {
            Monitor.Enter(monitorObject);
            if (!string.IsNullOrEmpty(Line)) {
                Add(Line + v, textColor);
                Line = "";
            } else {
                Add(v, textColor);
            }
            Monitor.Exit(monitorObject);
        }

        public void WriteLine(string v, ConsoleColor color) {
            Monitor.Enter(monitorObject);
            if (!string.IsNullOrEmpty(Line)) {
                Add(Line + v, color);
                Line = "";
            } else {
                Add(v, color);
            }
            Monitor.Exit(monitorObject);
        }

    }
}
