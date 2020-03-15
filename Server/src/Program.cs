using Microsoft.Xna.Framework;
using NLua;
using RunGun.Core;
using RunGun.Server.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace RunGun.Server
{
	class Program
	{
		static void Main(string[] args) {

			// create directories if they do not yet exist.
			System.IO.Directory.CreateDirectory("plugins");
			System.IO.Directory.CreateDirectory("logs");

			// copy serverconf to folder if doesn't exist yet.
			if (!System.IO.File.Exists("serverconf.lua")) {
				var sw = System.IO.File.CreateText("serverconf.lua");
				var data = FileUtils.ReadEmbedded("RunGun.Server.default-serverconf.lua");
				sw.Write(data);
				sw.Flush();
				sw.Close();
			}

			Server server = new Server(new IPEndPoint(IPAddress.Loopback, 22222));

			int exitCode = server.Run();

			if (exitCode != 0) {
				// TODO: make it yell about error?
			}
		}
	}
}