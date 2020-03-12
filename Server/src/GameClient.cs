using RunGun.Core;
using System.Net;

namespace RunGun.Server
{
	public class Client {

		public IPEndPoint endpoint;
		public string nickname;
		public double keepalive;
		public int id;
		//public Player character;

		bool kicked;

		public Client(IPEndPoint connectionPoint, string username, int suckmycock) {
			endpoint = connectionPoint;
			nickname = username;
			id = suckmycock;
			keepalive = 0;
			//character = new Player();
		}


		public void Kick() {
			kicked = true;
		}

	}
}