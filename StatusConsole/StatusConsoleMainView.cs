
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatusConsole {
    //public class StatusConsoleMainView : MainScreen<HostedCmdlineApp> {
    //    private const int C_TABSIZE = 4;
    //    private List<String> LineBuffer = new List<string>();
    //    private Size actualSize;

    //    public StatusConsoleMainView(int x, int y, HostedCmdlineApp model) : base(x, y, model) {
    //        actualSize = new Size(x, y);    
    //    }

    //    private int selIdx = -1;
    //    public override void HandleConsoleInput(Screen logScreen) {
    //        String line = "";
    //        int? editPos = null;
    //        while (line != "quit") {
    //            while (!Console.KeyAvailable) {
    //                bool refresh = false;
    //                if (Console.WindowWidth != this.actualSize.Width) {
    //                    refresh = true;
    //                    if (Console.WindowWidth < this.Size.Width) {
    //                        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
    //                            // in Windows, we are allowed to resize the hosting Console window
    //                            Console.WindowWidth = this.Size.Width;
    //                        } else {
    //                            // TODO:; Hmm was sollen wir in Linux hier tun !?.....
    //                        }
    //                    }
    //                    actualSize.Width = Console.WindowWidth;
    //                }
    //                if (Console.WindowHeight != this.actualSize.Height) {
    //                    refresh = true;
    //                    if (Console.WindowHeight < this.Size.Height) {
    //                        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
    //                            // in Windows, we are allowed to resize the hosting Console window
    //                            Console.WindowHeight = this.Size.Height;
    //                        } else {
    //                            // TODO:; Hmm was sollen wir in Linux hier tun !?.....
    //                        }
    //                    }
    //                    actualSize.Height = Console.WindowHeight;
    //                }
    //                if (refresh) {
    //                    Console.Clear();
    //                    Console.Write('x');     // This has to be done to 'reactivet Console Streams after Clear !?)
    //                    Fill(' ', deep: true);
    //                    logScreen.WriteLine("Resize done");     
    //                    ClearInputLine(line);
    //                    Console.Write(line);
    //                }
    //                Thread.Sleep(100);
    //            }

    //            ConsoleKeyInfo? k = Console.ReadKey(true);
           
    //            if (k != null) {
    //                if (k?.Key == ConsoleKey.Tab) {
    //                    var saveLeft = Console.CursorLeft;
    //                    Model.uartInFocus = Model._myServices.GetNextService();
    //                    ConsoleColor cc = Model.uartInFocus.IsConnected() ? ConsoleColor.Green : ConsoleColor.Red;
    //                    logScreen.Write("Switched to ");
    //                    logScreen.WriteLine(Model.uartInFocus.GetInterfaceName(), cc);
    //                    ClearInputLine(line);
    //                    Console.Write(line);
    //                } else if (k?.Key == ConsoleKey.Enter) {
    //                    Model.uartInFocus?.SendUart(line);
    //                    LineBuffer.Add(line);
    //                    selIdx = -1;
    //                    ClearInputLine(line);
    //                    line = "";
    //                    editPos = null;
    //                } else if (k?.Key == ConsoleKey.Escape) {
    //                    Console.Clear();
    //                    Console.Write('x');     // This has to be done to 'reactivet Console Streams after Clear !?)
    //                    Fill(' ', deep: true);
    //                    logScreen.WriteLine("Clear done");
    //                    ClearInputLine(line);
    //                    Console.Write(line);
    //                } else if (k?.Key == ConsoleKey.UpArrow) {
    //                    if (LineBuffer.Count > 0) {
    //                        if (selIdx == -1) {
    //                            selIdx = LineBuffer.Count;
    //                        }
    //                        selIdx--;
    //                        if (selIdx >= 0) {
    //                            ClearInputLine(line);
    //                            line = LineBuffer[selIdx];
    //                            Console.Write(line);
    //                        }
    //                    }
    //                } else if (k?.Key == ConsoleKey.DownArrow) {
    //                    if (selIdx >= 0) {
    //                        selIdx++;
    //                    }
    //                    if (selIdx < LineBuffer.Count) {
    //                        ClearInputLine(line);
    //                        line = LineBuffer[selIdx];
    //                        Console.Write(line);

    //                    } else {
    //                        selIdx = 0;
    //                    }
    //                } else if (k?.Key == ConsoleKey.Backspace) {
    //                    ClearInputLine(line);
    //                    if (editPos == null) {
    //                        if (line.Length > 0) {
    //                            line = line.Substring(0, line.Length - 1);
    //                            Console.Write(line);
    //                            Console.SetCursorPosition(InputPos.Left + line.Length, InputPos.Top);
    //                        }
    //                    } else {
    //                        if (editPos > 0) { 
    //                            int x = (int)editPos;
    //                            line = line.Remove(x - 1, 1);
    //                            Console.Write(line);
    //                            editPos--;
    //                            Console.SetCursorPosition(InputPos.Left + x - 1, InputPos.Top);
    //                        }
    //                    }
    //                } else if (k?.Key == ConsoleKey.LeftArrow) {
    //                    if (editPos == null) {
    //                        editPos = line.Length - 1;
    //                    } else {
    //                        editPos--;
    //                    }
    //                    if (editPos < 0) {
    //                        editPos = 0;
    //                    }
    //                    Console.SetCursorPosition(InputPos.Left+editPos??0, InputPos.Top);
    //                } else if (k?.Key == ConsoleKey.RightArrow) {
    //                    if (editPos != null) {
    //                        editPos++;
    //                        Console.SetCursorPosition(InputPos.Left + editPos ?? 0, InputPos.Top);
    //                        if (editPos >= line.Length) {
    //                            editPos = null;
    //                        }
    //                    }
    //                } else {
    //                    Console.Write(k?.KeyChar);
    //                    if (editPos == null) {
    //                        line += k?.KeyChar;
    //                        //Console.Write(k?.KeyChar);
    //                    } else {
    //                        int x = (int)editPos;
    //                        string y = k?.KeyChar.ToString();
    //                        line = line.Remove(x, 1).Insert(x, y);
    //                        editPos++;
    //                        if (editPos >= line.Length) {
    //                            editPos = null;
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //        logScreen.WriteLine("Input Handler closed!");
    //        Model.StopAsync(new System.Threading.CancellationToken()).Wait();
    //        Environment.Exit(-22);
    //    }

    //    private void ClearInputLine(string line) {
    //        Console.SetCursorPosition(InputPos.Left, InputPos.Top);
    //        Console.Write("".PadLeft(line.Length));
    //        Console.SetCursorPosition(InputPos.Left, InputPos.Top);
    //    }
    //}
}


