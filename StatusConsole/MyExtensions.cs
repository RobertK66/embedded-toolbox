using ConsoleGUI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    internal static class MyExtensions {
        public static Task ForEachAsync<T>(this IEnumerable<T> sequence, Func<T, Task> action) {
            return Task.WhenAll(sequence.Select(action));
        }


        public static Color GetGuiColor(this ConsoleColor color) {
            if (color == ConsoleColor.DarkGray) return new Color(128, 128, 128);
            if (color == ConsoleColor.Gray) return new Color(192, 192, 192);
            int index = (int)color;
            byte d = ((index & 8) != 0) ? (byte)255 : (byte)128;
            return new Color(
                ((index & 4) != 0) ? d : (byte)0,
                ((index & 2) != 0) ? d : (byte)0,
                ((index & 1) != 0) ? d : (byte)0);
        }
    }
}
