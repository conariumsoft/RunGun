#define RENDERING
#define DESKTOP_GL

using System;
using System.Collections.Generic;
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

            string nickname = "player";
            string ip = "127.0.0.1";
            string port = "22222";
            Thread.Sleep(200);
            ClientMain game = new ClientMain(nickname, ip, port);

			GLChatSystem chat = new GLChatSystem(game.OnPlayerSendChat);
			game.chat = chat;

            game.Window.TextInput += chat.OnTextInput;            

            game.Run();

			game.Dispose();
			return;
        }
    }
}
