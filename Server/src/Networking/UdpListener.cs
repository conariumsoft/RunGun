using RunGun.Core.Networking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RunGun.Server.Networking
{
	class UdpListener : UdpBase
	{
		private IPEndPoint _listenOn;

		public UdpListener() : this(new IPEndPoint(IPAddress.Loopback, 12345)) {

		}

		public UdpListener(IPEndPoint endpoint) {
			_listenOn = endpoint;
			Client = new UdpClient(_listenOn);
		}

		public void Reply(string message, IPEndPoint endpoint) {
			var datagram = Encoding.ASCII.GetBytes(message);

			Client.Send(datagram, datagram.Length, endpoint);
		}
	}
}
