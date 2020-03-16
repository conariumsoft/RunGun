using System;
using System.Threading;

namespace RunGun.Client
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Console.WriteLine("Starting game client...");
            Thread.Sleep(200);
            using (var game = new ClientMain()) {
                game.Run();
            }
        }
    }
}