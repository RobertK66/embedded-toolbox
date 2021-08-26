using System;
using ScreenLib;

namespace StatusConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hellö Würld!");

            ScreenArea sca = new ScreenArea();
            sca.SayHello();
            string v = Console.ReadLine();
        }
    }

}
