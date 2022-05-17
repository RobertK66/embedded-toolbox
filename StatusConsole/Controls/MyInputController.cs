using ConsoleGUI.Controls;
using ConsoleGUI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.Controls {
    public class MyInputController : IInputListener {
        private const string Prefix = ">";
        private List<string> lines = new List<string>();
        private int curLine = -1;
        private TextBox textBox;
        private Action<string> callback;

        public MyInputController(TextBox textBox, Action<string> commandCallback) {
            this.textBox = textBox;
            this.textBox.Text = Prefix;
            this.textBox.Caret = Prefix.Length;
            callback = commandCallback;
        }

        public void OnInput(InputEvent inputEvent) {
			if (inputEvent.Key.Key == ConsoleKey.Enter) {
                string cmdLine = this.textBox.Text.Substring(Prefix.Length);
                lines.Add(cmdLine);
                curLine = lines.Count - 1;
                textBox.Text = Prefix;
                callback(cmdLine);
				inputEvent.Handled = true;
			} else if (inputEvent.Key.Key == ConsoleKey.UpArrow) {
                if (curLine >= 0) {
                    textBox.Text = Prefix + lines[curLine--];
                    if (curLine < 0) {
                        curLine = 0;
                    }
                }
                inputEvent.Handled = true;
            } else if (inputEvent.Key.Key == ConsoleKey.DownArrow) {
                if (curLine < lines.Count - 1) {
                    textBox.Text = Prefix + lines[curLine++];
                    if (curLine > lines.Count - 1) {
                        curLine = lines.Count - 1;
                    }
                }
                inputEvent.Handled = true;
            }

		}
    }

}
