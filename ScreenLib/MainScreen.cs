using System;
using System.Runtime.InteropServices;

namespace ScreenLib {
    public class LineEnteredArgs : EventArgs {
        public String Cmd { get; set; }
    }


    public abstract class MainScreen<T> : Screen {

        public event EventHandler<LineEnteredArgs> LineEntered;
        protected T Model;
       

        protected Position InputPos;
        public MainScreen(int x, int y, T model) : base(x, y, null) {
            this.Parent = this;
            Console.Title = String.Format("Screen {0} - {1} ", Size.Width, Size.Height);
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                Console.WindowWidth = x;
                Console.WindowHeight = y;
            }
            WritePosition(Size.Width - 2, Size.Height - 2, "+");
            InputPos = new Position(0, Size.Height - 2);
            Console.CursorLeft = InputPos.Left;
            Console.CursorTop = InputPos.Top;
            Model = model;
        }

        public override void Clear(bool deep) {
            base.Clear(deep);
            WritePosition(Size.Width - 2, Size.Height - 2, "*");
        }

        public void RunLine(String exitCmd) {
            string v = "";
            while(!v.Equals(exitCmd)) {
                Console.SetCursorPosition(InputPos.Left, InputPos.Top);
                v = Console.ReadLine();
                if(v.Equals("clr")) {
                    Clear(true);
                } else {
                    String line = "";
                    line = line.PadLeft(this.Size.Width - InputPos.Left);
                    WritePosition(InputPos.Left, InputPos.Top, line);
                    LineEntered?.Invoke(this, new LineEnteredArgs() { Cmd = v });
                }
            }

        }

        abstract public void HandleConsoleInput(Screen inputScreen, string debugOption, int sleep);
  
    }
}
