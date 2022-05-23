using ConsoleGUI.Controls;
using ConsoleGUI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.Controls {
    public class MyInputController : IInputListener {
        
        private List<List<string>> buffers = new List<List<string>>();
        private List<int> curLines = new List<int>();
        private List<TextBox> textBoxes = new List<TextBox>();
        private List<string> Prefix = new List<string>();
        private List<Action<string>> callbacks = new List<Action<string>>();

        private int currentActiveIdx = -1;
        public MyInputController() {
        }

        public int AddCommandLine(TextBox textBox, Action<string> commandCallback, string prefix = ">") {
            textBox.Text = prefix;
            textBox.Caret = prefix.Length;

            textBoxes.Add(textBox);
            callbacks.Add(commandCallback);
            Prefix.Add(prefix);
            buffers.Add(new List<string>());
            curLines.Add(-1);

            currentActiveIdx = textBoxes.Count - 1;
            return currentActiveIdx;     // return the index
        }

        public void SetActive(int activeIdx) {
            if (activeIdx >= 0 && activeIdx < textBoxes.Count)
                this.currentActiveIdx = activeIdx;
        }

        public void OnInput(InputEvent inputEvent) {
            if (currentActiveIdx >= 0 && currentActiveIdx < textBoxes.Count) {
                TextBox tb = textBoxes[currentActiveIdx];
                String pr = Prefix[currentActiveIdx];
                List<string> lines = buffers[currentActiveIdx];
                Action<string> callback = callbacks[currentActiveIdx];

                if (inputEvent.Key.Key == ConsoleKey.Enter) {
                    if (tb.Text.Length >= pr.Length) {
                        string cmdLine = tb.Text.Substring(pr.Length);
                        lines.Add(cmdLine);
                        curLines[currentActiveIdx] = lines.Count - 1;
                        callback(cmdLine);
                    }
                    inputEvent.Handled = true;
                    tb.Text = pr;
                    tb.Caret = pr.Length;
                } else if (inputEvent.Key.Key == ConsoleKey.UpArrow) {
                    if (curLines[currentActiveIdx] >= 0) {
                        tb.Text = pr + lines[curLines[currentActiveIdx]--];
                        if (curLines[currentActiveIdx] < 0) {
                            curLines[currentActiveIdx] = 0;
                        }
                    }
                    tb.Caret = tb.Text.Length;
                    inputEvent.Handled = true;
                } else if (inputEvent.Key.Key == ConsoleKey.DownArrow) {
                    if (curLines[currentActiveIdx] < lines.Count - 1) {
                        tb.Text = pr + lines[curLines[currentActiveIdx]++];
                        if (curLines[currentActiveIdx] > lines.Count - 1) {
                            curLines[currentActiveIdx] = lines.Count - 1;
                        }
                    }
                    tb.Caret = tb.Text.Length;
                    inputEvent.Handled = true;
                } else {
                    // pass all other events to the current active text box. It handles rest of line edit.
                    ((IInputListener)tb).OnInput(inputEvent);
                }
            }
		}
    }

}
