using System;

namespace ScreenLib
{
    public class Position {
        public int Left {get; set;}
        public int Top {get; set;}

        public Position(int x, int y) {
            Left = x;
            Top = y;
        }
    }
}