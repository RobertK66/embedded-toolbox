using System;
using ScreenLib;

namespace StatusConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Run();

        }

        private Screen sca;
        private Screen scb;
        private Screen scc;

        public void Run() {
            MainScreen main = new MainScreen(110,30);
            sca = main.AddScreen(0,0, new Screen(50, 10));
            scb = main.AddScreen(50,0, new Screen(50, 10, ConsoleColor.Yellow, ConsoleColor.Black));
            scc = main.AddScreen(0,14, new Screen(100, 14, ConsoleColor.DarkBlue, ConsoleColor.White));
            scb.VertType = VerticalType.WRAP_AROUND;
            scb.HoriType = HorizontalType.WRAP;
            main.Clear(true);

            sca.WriteLine("Hallo Screen A");
            scb.WriteLine("Hi to Screen B");
            scc.WriteLine("---- Trööööt -------");
            sca.WriteLine("---------------");
            scb.WriteLine("**************");
            scc.WriteLine("----------------------------------------> Schirm C hier");

            main.LineEntered += LineHandler;

            main.RunLine("quit");

        }

        public void LineHandler(object sender, LineEnteredArgs e) {
            scb.WriteLine(e.Cmd);
            scb.WritePosition(40, 3, e.Cmd.ToUpper(), 7);
        }

    }

}
