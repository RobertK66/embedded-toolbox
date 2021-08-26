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
            sca = new Screen(50,10, main);
            scb = new Screen(50,10, main, ConsoleColor.Yellow,  ConsoleColor.Black );
            scc = new Screen(100,14, main, ConsoleColor.DarkBlue,  ConsoleColor.White ) ;
            main.AddScreen(0,0,sca);
            main.AddScreen(50,0,scb);
            main.AddScreen(0,14,scc);

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
        }

    }

}
