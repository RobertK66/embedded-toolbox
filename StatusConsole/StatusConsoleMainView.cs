using ScreenLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {
    public class StatusConsoleMainView : MainScreen<HostedCmdlineApp> {
        private const int C_TABSIZE = 4;
        private List<String> LineBuffer = new List<string>();

        public StatusConsoleMainView(int x, int y, HostedCmdlineApp model) : base(x, y, model) {

        }


        private int selIdx = -1;
        public override void HandleConsoleInput(Screen logScreen, String debugOption, int sleep) {
            String line = "";
            while(line != "quit") {
                ConsoleKeyInfo? k = null;
                if (debugOption.Equals("A")) {
                    k = Console.ReadKey(true);
                } else if (debugOption.Equals("B")) {
                    k = Console.ReadKey(false);
                } else if (debugOption.Equals("C")) {
                    if (Console.KeyAvailable) {
                        k = Console.ReadKey(false);
                    } else {
                        Thread.Sleep(sleep);
                    }
                } else {
                    while (!Console.KeyAvailable) {
                        Thread.Sleep(sleep);
                    }
                    k = Console.ReadKey(false);
                }

                if (k != null) {

                    if (k?.Key == ConsoleKey.Tab) {
                        var saveLeft = Console.CursorLeft;
                        Model.uartInFocus = Model._myServices.GetNextService();
                        ConsoleColor cc = Model.uartInFocus.IsConnected() ? ConsoleColor.Green : ConsoleColor.Red;
                        logScreen.Write("Switched to ");
                        logScreen.WriteLine(Model.uartInFocus.GetInterfaceName(), cc);
                        ClearInputLine(line);
                        Console.Write(line);
                    } else if (k?.Key == ConsoleKey.Enter) {
                        Model.uartInFocus?.SendUart(line);
                        LineBuffer.Add(line);
                        selIdx = -1;
                        ClearInputLine(line);
                        line = "";
                    } else if (k?.Key == ConsoleKey.Escape) {
                        Clear(true);    //Todo: !? geht nicht mehr !?
                    } else if (k?.Key == ConsoleKey.UpArrow) {
                        if (LineBuffer.Count > 0) {
                            if (selIdx == -1) {
                                selIdx = LineBuffer.Count;
                            }
                            selIdx--;
                            if (selIdx >= 0) {
                                ClearInputLine(line);
                                line = LineBuffer[selIdx];
                                Console.Write(line);
                            }
                        }
                    } else if (k?.Key == ConsoleKey.DownArrow) {
                        if (selIdx >= 0) {
                            selIdx++;
                        }
                        if (selIdx < LineBuffer.Count) {
                            ClearInputLine(line);
                            line = LineBuffer[selIdx];
                            Console.Write(line);
                        } else {
                            selIdx = 0;
                        }
                    } else {
                        line += k?.KeyChar;
                    }
                }
            }
            logScreen.WriteLine("Input Handler closed!");
            _ = Model.StopAsync(new System.Threading.CancellationToken());
        }

        private void ClearInputLine(string line) {
            Console.SetCursorPosition(InputPos.Left, InputPos.Top);
            Console.Write("".PadLeft(line.Length));
            Console.SetCursorPosition(InputPos.Left, InputPos.Top);
        }
    }
}
