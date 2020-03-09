using RunGun.Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunGun.Client.Networking
{
	class UdpUser : UdpBase
	{

		private UdpUser() { }

		public static UdpUser Connect(string hostname, int port) {
			var connection = new UdpUser();
			connection.Client.Connect(hostname, port);
			return connection;
		}

		public void Send(string message) {
			var datagram = Encoding.ASCII.GetBytes(message);

			Client.Send(datagram, datagram.Length);
		}
	}
}
