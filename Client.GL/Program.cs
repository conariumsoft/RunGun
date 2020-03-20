#define RENDERING
using System;
using System.Threading;
using RunGun.Client;

namespace RunGun.GLClient
{
    public static class Program
    {


        [STAThread]
        static void Main(string[] args)
        {
            /* If you run into trouble building for linux/OS-X relating to Steamworks.NET.dll
             * read this
             * https://steamworks.github.io/installation/
             */
            //SteamAPI.Init(); not finding steam_api64.dll oddly...

            string nickname = "player";
            string ip = "127.0.0.1";
            string port = "22222";
            Thread.Sleep(200);
            using (var game = new ClientMain(nickname, ip, port))
                game.Run();
        }
    }
}
