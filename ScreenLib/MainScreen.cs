using System;

namespace ScreenLib
{
    public class LineEnteredArgs :EventArgs {
        public String Cmd {get; set;}
    }


    public class MainScreen : Screen {

        public event EventHandler<LineEnteredArgs> LineEntered;

        private Position InputPos;
        public MainScreen (int x, int y) : base(x, y, null) {
            this.Parent = this;
            Console.Title = "Screen X - Y ";
            WritePosition(Size.Width -1, Size.Height - 1, "+");
            InputPos = new Position(0,Size.Height - 2);
            Console.CursorLeft = InputPos.Left;
            Console.CursorTop = InputPos.Top;
        }

        public override void Clear(bool deep) {
            base.Clear(deep);
            WritePosition(Size.Width -1, Size.Height - 1, "+");
        }

        public void RunLine(String exitCmd) {
            string v = "";
            while (!v.Equals(exitCmd)) {
                Console.SetCursorPosition(InputPos.Left, InputPos.Top);
                v = Console.ReadLine();
                if (v.Equals("clr")) {
                    Clear(true);
                } else {
                    String line = "";
                    line = line.PadLeft(this.Size.Width - InputPos.Left);
                    WritePosition(InputPos.Left, InputPos.Top, line);
                    LineEntered?.Invoke(this, new LineEnteredArgs() {Cmd = v});
                }
           }

        }
    }
}