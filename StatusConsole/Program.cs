using System;
using ScreenLib;

namespace StatusConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            ScreenArea sca = new ScreenArea();
            sca.SayHello();
            Console.ReadLine();
        }
    }
}
