using RunGun.Core;
using System.Net;

namespace RunGun.Server
{
	public class Client
	{

		public IPEndPoint endpoint;
		public string nickname;
		public double keepalive;
		public int id;
		public Player character;

		public Client(IPEndPoint connectionPoint, string username) {
			endpoint = connectionPoint;
			nickname = username;
			id = 1;
			keepalive = 0;
			character = new Player();
		}
	}
}