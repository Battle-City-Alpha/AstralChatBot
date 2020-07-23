using System;

namespace AstralBot
{
    class Program
    {
        public static string path = AppDomain.CurrentDomain.BaseDirectory;

        static void Main(string[] args)
        {
            UselessThings();
            Bot b = new Bot();
        }

        static void UselessThings()
        {
            Console.WriteLine("ASTRAL  BOT - V0.0.1");
        }
    }
}
