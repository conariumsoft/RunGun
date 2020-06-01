using RunGun.Server.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace RunGun.Server
{
	[Serializable]
	public class ServerConfiguration
	{
		[XmlIgnore]
		private const string configFileName = "Config.xml";

		public string ListenAddress { get; set; } = "";
		public int ListenPort { get; set; } = 22222;
		public int MaxPlayers { get; set; } = 32;
		public string ServerName { get; set; } = "Server Mk1";
		public int MinimumThreadSlepTime { get; set; } = 8;
		public float GameStateTickRate { get; set; } = 30.0f;
		public float UsersTimeoutAfter { get; set; } = 10.0f;

		public static void CreateDefaultConfig() {
			// copy serverconf to folder if doesn't exist yet.
			if (!System.IO.File.Exists(configFileName)) {
				using (var writer = new System.IO.StreamWriter(configFileName)) {
					var serializer = new XmlSerializer(typeof(ServerConfiguration));
					serializer.Serialize(writer, new ServerConfiguration());
					writer.Flush();
				}
			}
		}

		public static ServerConfiguration Load() {
			using (var stream = System.IO.File.OpenRead(configFileName)) {
				var serializer = new XmlSerializer(typeof(ServerConfiguration));
				return serializer.Deserialize(stream) as ServerConfiguration;
			}
		}
	}
}
