using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client;
using RunGun.Client.Misc;

namespace RunGun.GLClient
{
	public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            /* If you run into trouble building for linux/OS-X relating to Steamworks.NET.dll
             * read this https://steamworks.github.io/installation/
             */

            string ip = "127.0.0.1";
            int port = 22222;
            Thread.Sleep(500);
            using (GLClient game = new GLClient()) {
                game.Nickname = "glplayer";
                game.ConnectToServer(new IPEndPoint(IPAddress.Parse(ip), port));
                game.Run();
            }

                

			//game.Dispose();
        }
    }
}
