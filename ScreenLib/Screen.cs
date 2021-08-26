using System;
using System.Collections.Generic;


namespace ScreenLib
{
    public class Screen {

        public VerticalType VertType { get; set; } = VerticalType.OVERWRITE_LAST_LINE;
        public HorizontalType HoriType { get; set; } = HorizontalType.CUT;
        public Boolean FillUpLine { get; set; } = true;

        public Size Size {get; set; }
     
        public Position CursorPos {get; set;}
        public Position Offset {get; set;}
        public ConsoleColor BackgroundColor { get; set; }
        public ConsoleColor TextColor { get; set; }
        
        
        protected Screen Parent;
        protected List<Screen> Children = new List<Screen>();

        public Screen(int cols, int rows, Nullable<ConsoleColor> background = null, Nullable<ConsoleColor> textcol = null) {
            //Parent = parent;
            Size = new Size(cols, rows);
            BackgroundColor = background ?? ConsoleColor.Black;
            TextColor = textcol ?? ConsoleColor.White;
            CursorPos = new Position(0,0);
            Offset = new Position(0,0);
        }

        public Screen AddScreen(int xPos, int yPos, Screen child) {
            if (xPos + child.Size.Width > this.Size.Width) {
                // Passt nicht -> resize/cut ???
            } 
            if (yPos + child.Size.Height > this.Size.Height) {
                // Passt nicht -> resize/cut ???
            }
            // Overlapping childeren -> ????
            Children.Add(child);
            child.Offset = new Position(xPos, yPos);
            child.Parent = this;
            return child;
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
            if(CursorPos.Top >= 0) {
                String tail = String.Empty;
                var cs = GetConsoleState();
                if ((CursorPos.Top == 0) && (VertType == VerticalType.RESTART)) {
                    Clear();
                }
                if(message.Length > (Size.Width - CursorPos.Left)) {
                    String toPrint = message.Substring(0, (Size.Width - CursorPos.Left));
                    switch(HoriType) {
                    case HorizontalType.CUT:
                        message = toPrint;
                        break;
                    case HorizontalType.WRAP:
                    case HorizontalType.WRAP_OVER:
                        tail = message.Substring((Size.Width - CursorPos.Left));
                        message = toPrint;
                        break;
                    }
                } else {
                    message = message.PadRight(Size.Width);
                }
                Console.SetCursorPosition(Offset.Left + CursorPos.Left, Offset.Top + CursorPos.Top);
                Console.BackgroundColor = this.BackgroundColor;
                Console.ForegroundColor = this.TextColor;
                Console.Write(message);
                CursorPos.Top++;
                if(CursorPos.Top > Size.Height - 1) {
                    // we are in last line now. What to do:
                    switch(VertType) {
                    case VerticalType.SCROLL:
                        // Not possible withourt buiffer -> Todo
                        // make Last line repeat for now ....
                    case VerticalType.OVERWRITE_LAST_LINE:
                        CursorPos.Top--;
                        break;  

                    case VerticalType.CUT:
                        // Stop writing until somebody resets Cursor Pos
                        CursorPos.Top = -1;
                        break;

                    case VerticalType.WRAP_AROUND:
                    case VerticalType.RESTART:
                        CursorPos.Top = 0;
                        break;
                    }
                }
                RestoreConsoleState(cs);
                if (!String.IsNullOrEmpty(tail)) {
                    if (HoriType == HorizontalType.WRAP_OVER) {
                        CursorPos.Top--;
                    }
                    WriteLine(tail);
                }
            }
        }

        public void WritePosition(int x, int y, String message, int fieldLen = -1) {
            if((x < Size.Width - 1) && (y < Size.Height - 1)) {
                int allowedLen = (Size.Width - x);
                if(fieldLen > 0) {
                    allowedLen = Math.Min(allowedLen, fieldLen);
                    message = message.PadRight(fieldLen);
                }

                if(message.Length > allowedLen) {
                    message = message.Substring(0, allowedLen);
                }
                var cs = GetConsoleState();
                Console.SetCursorPosition(Offset.Left + x, Offset.Top + y);
                Console.BackgroundColor = this.BackgroundColor;
                Console.ForegroundColor = this.TextColor;
                Console.Write(message);
                RestoreConsoleState(cs);
            }
        }

        public virtual void Clear(bool deep=false) {
            var cs = GetConsoleState();
            String line = "";
            line = line.PadLeft(this.Size.Width);
            Console.BackgroundColor = this.BackgroundColor;
            for (int l = Offset.Top; l< Offset.Top + Size.Height; l++) {
                Console.SetCursorPosition(Offset.Left, l);    
                Console.Write(line);
            }
            CursorPos.Top = 0;
            RestoreConsoleState(cs);

            if (deep) {
                foreach(Screen child in Children) {
                    child.Clear(deep);
                }
            }
        }
    }
}
