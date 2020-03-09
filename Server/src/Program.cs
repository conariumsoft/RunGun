using Microsoft.Xna.Framework;
using RunGun.Core;
using System.Net;

namespace RunGun.Server
{

	public static class Config
	{
		public const int MAX_CLIENTS = 64;
	}

	public class LevelGeometry : Entity
	{
		public Vector2 size;

		public LevelGeometry() {

		}

		public LevelGeometry(Vector2 pos, Vector2 sz) {
			Position = pos;
			size = sz;
		}

		public Vector2 GetCenter() {
			return Position + (size / 2);
		}

		public Vector2 GetDimensions() {
			return size / 2;
		}
	}

	class Program
	{
		static void Main(string[] args) {
			Server server = new Server(new IPEndPoint(IPAddress.Loopback, 12345));

			int exitCode = server.Run();

			if (exitCode != 0) {
				// TODO: make it yell about error?
			}
		}
	}
}