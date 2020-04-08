using RunGun.Core.Utility;
using RunGun.Server.Utils;
using System.Net;

namespace RunGun.Server
{
	class ServerProgram
	{
		static void CreateDefaultConfiguration() {
			// copy serverconf to folder if doesn't exist yet.
			if (!System.IO.File.Exists("serverconf.lua")) {
				var sw = System.IO.File.CreateText("serverconf.lua");
				var data = FileUtils.ReadEmbedded("RunGun.Server.LuaScripts.default-serverconf.lua");
				sw.Write(data);
				sw.Flush();
				sw.Close();
			}
		}
		static void CreatePluginFolder() {
			System.IO.Directory.CreateDirectory("plugins");
		}
		static void CreateLogsFolder() {
			System.IO.Directory.CreateDirectory("logs");
		}
		static void LoadConfig() {}

		static void Main(string[] args) {
			Logging.Out("Server Bootstrap...");
			CreateDefaultConfiguration();
			CreatePluginFolder();
			CreateLogsFolder();

			LoadConfig();

			Server server = new Server(new IPEndPoint(IPAddress.Any, 22222)) {
				MaxPlayers = 32,
				ServerName = "Server MkI"
			};

			int exitCode = server.Run();
			Logging.Out("Server exited with code "+exitCode);
			return;
		}
	}
}