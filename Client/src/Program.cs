using System;

namespace RunGun.Client
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {

            Console.WriteLine("ASS");

            using (var game = new Game1()) {
                game.Run();
            }

        }
    }
}