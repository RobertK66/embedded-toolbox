using ConsoleGUI.Controls;
using ConsoleGUI.Data;
using ConsoleGUI.Space;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole.Controls {
    public class ForceForeground : Decorator {
        private Color myforeground;

        public ForceForeground(Color c) { 
            myforeground = c;
        }

        public override Cell this[Position position] => Content[position].WithForeground(myforeground);



    }
}
