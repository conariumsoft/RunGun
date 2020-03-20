using Microsoft.Xna.Framework;
using NLua;
using RunGun.Core;
using RunGun.Server.Utils;
using System.Net;

namespace RunGun.Server
{
	class Program
	{
		static void CreateServerFiles() {
			// create directories if they do not yet exist.
			System.IO.Directory.CreateDirectory("plugins");
			System.IO.Directory.CreateDirectory("logs");

			// copy serverconf to folder if doesn't exist yet.
			if (!System.IO.File.Exists("serverconf.lua")) {
				var sw = System.IO.File.CreateText("serverconf.lua");
				var data = FileUtils.ReadEmbedded("RunGun.Server.LuaScripts.default-serverconf.lua");
				sw.Write(data);
				sw.Flush();
				sw.Close();
			}
		}

		static void LoadConfig() {}

		static void Main(string[] args) {
			Logging.Out("Starting server...");
			CreateServerFiles();

			Server server = new Server(new IPEndPoint(IPAddress.Any, 22222));

			int exitCode = server.Run();
			if (exitCode != 0) {
				// TODO: make it yell about error?
				Logging.Out("Starting ran fine.");
			} else {
				Logging.Out("Server probably crashed.");
			}
		}
	}
}