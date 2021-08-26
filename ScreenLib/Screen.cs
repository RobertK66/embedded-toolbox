using System;
using System.Collections.Generic;


namespace ScreenLib
{
    public class Screen
    {

        public Size Size {get; set; }
     
        public Position CursorPos {get; set;}
        public Position Offset {get; set;}
        public ConsoleColor BackgroundColor { get; set; }
        public ConsoleColor TextColor { get; set; }
        
        
        protected Screen Parent;
        protected List<Screen> Children = new List<Screen>();

        public Screen(int cols, int rows, Screen parent, Nullable<ConsoleColor> background = null, Nullable<ConsoleColor> textcol = null) {
            Parent = parent;
            Size = new Size(cols, rows);
            BackgroundColor = background ?? ConsoleColor.Black;
            TextColor = textcol ?? ConsoleColor.White;
            CursorPos = new Position(0,0);
            Offset = new Position(0,0);
        }

        public void AddScreen(int xPos, int yPos, Screen child) {
            if (xPos + child.Size.Width > this.Size.Width) {
                // Passt nicht -> resize/cut ???
            } 
            if (yPos + child.Size.Height > this.Size.Height) {
                // Passt nicht -> resize/cut ???
            }
            // Overlapping childeren -> ????
            Children.Add(child);
            child.Offset = new Position(xPos, yPos);
        }

        private (int left, int top, ConsoleColor back, ConsoleColor fore) GetConsoleState() {
            var cp = Console.GetCursorPosition();
            return (cp.Left, cp.Top, Console.BackgroundColor, Console.ForegroundColor);
        }

        private void RestoreConsoleState((int left, int top, ConsoleColor back, ConsoleColor fore) cs) {
            Console.SetCursorPosition(cs.left, cs.top);
            Console.BackgroundColor = cs.back;
            Console.ForegroundColor = cs.fore;
        }


        public void WriteLine(String message) {
            var cs = GetConsoleState();
            Console.SetCursorPosition(Offset.Left + CursorPos.Left, Offset.Top + CursorPos.Top);
            Console.BackgroundColor = this.BackgroundColor;
            Console.ForegroundColor = this.TextColor;
            Console.Write(message);
            this.CursorPos.Top++;
            RestoreConsoleState(cs);
        }

        public void WritePosition(int x, int y, String message) {
            var cs = GetConsoleState();
            Console.SetCursorPosition(Offset.Left + x, Offset.Top + y);
            Console.BackgroundColor = this.BackgroundColor;
            Console.ForegroundColor = this.TextColor;
            Console.Write(message);
            RestoreConsoleState(cs);
        }

        public virtual void Clear(bool deep=false) {
            String line = "";
            line = line.PadLeft(this.Size.Width);
            var cp = Console.GetCursorPosition();
            Console.BackgroundColor = this.BackgroundColor;
            for (int l = Offset.Top; l< Offset.Top + Size.Height; l++) {
                Console.SetCursorPosition(Offset.Left, l);    
                Console.Write(line);
            }
            Console.SetCursorPosition(cp.Left, cp.Top);
            if (deep) {
                foreach(Screen child in Children) {
                    child.Clear(deep);
                }
            }
        }
    }
}
