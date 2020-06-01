using RunGun.Core.Utility;
using RunGun.Server.Utils;
using System.Diagnostics;
using System.Net;

namespace RunGun.Server
{
	class Program
	{
		[Conditional("DEBUG")]
		private static void RuntimeTesting() {
			Core.Networking.TypeSerializer.TestTypeRoundTrip();
		}
		private static void CreatePluginFolder() {
			System.IO.Directory.CreateDirectory("plugins");
		}
		private static void CreateLogsFolder() {
			System.IO.Directory.CreateDirectory("logs");
		}

		static void Main(string[] args) {
			RuntimeTesting();
			Logging.Out("Server Bootstrap...");
			ServerConfiguration.CreateDefaultConfig();
			CreatePluginFolder();
			CreateLogsFolder();

			ServerConfiguration config = ServerConfiguration.Load();

			Server server = new Server() {
				MinimumThreadSleepTime = config.MinimumThreadSlepTime,
				GameStateTickRate = config.GameStateTickRate,
				UsersTimeoutAfter = config.UsersTimeoutAfter,
				MaxPlayers = config.MaxPlayers,
				ServerName = config.ServerName,
			};
			server.BindTo(new IPEndPoint(IPAddress.Any, config.ListenPort));

			int exitCode = server.Run();
			Logging.Out("Server exited with code "+exitCode);
			return;
		}
	}
}