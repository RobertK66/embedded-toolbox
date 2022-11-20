using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using StatusConsoleApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if (v.Contains("\r")) {
                Line += v.Substring(0, v.IndexOf("\r"));
                Add(Line, textColor);
                Line = v.Substring(v.IndexOf("\r") + 1);
            } else {
                Line += v;
            }
        }

        public void WriteData(byte[] buffer, int bytesRead) {
            Add(buffer.ToString(), textColor);
        }

        public void WriteLine(string v) {
            if (!string.IsNullOrEmpty(Line)) {
                Add(Line + v, textColor);
                Line = "";
            } else {
                Add(v, textColor);
            }
        }

        public void WriteLine(string v, ConsoleColor color) {
            if (!string.IsNullOrEmpty(Line)) {
                Add(Line + v, color);
                Line = "";
            } else {
                Add(v, color);
            }
        }

    }
}
