using ScreenLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusConsole {
    public class StatusConsoleMainView : MainScreen<HostedCmdlineApp> {
        public StatusConsoleMainView(int x, int y, HostedCmdlineApp model) : base(x, y, model) {
        }

        public override void HandleConsoleInput(Screen inputScreen) {
            String line = "";
            while(line != "quit") {
                ConsoleKeyInfo k = Console.ReadKey();
                if(k.Key == ConsoleKey.Tab) {
                    Model.uartInFocus = Model._myServices.GetNextService();
                    ConsoleColor cold = inputScreen.TextColor;
                    ConsoleColor cc = Model.uartInFocus.IsConnected() ? ConsoleColor.Green : ConsoleColor.Red;
                    inputScreen.Write("Switched to ");
                    inputScreen.TextColor = cc;
                    inputScreen.WriteLine(Model.uartInFocus.GetInterfaceName());
                    inputScreen.TextColor = cold;
                    //Console.SetCursorPosition(inputScreen.CursorPos.Left, inputScreen.CursorPos.Top);
                } else if(k.Key == ConsoleKey.Enter) {
                    Model.uartInFocus?.SendUart(line);
                    line = "";
                } else {
                    line += k.KeyChar;
                }
            }
            inputScreen.WriteLine("Input Handler closed!");
        }
    }
}
