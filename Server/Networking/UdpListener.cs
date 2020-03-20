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

		public UdpListener(IPEndPoint endpoint) {
			_listenOn = endpoint;
			Client = new UdpClient(_listenOn);
		}

		public void Reply(byte[] packet, IPEndPoint endpoint) {
			try {
				Client.Send(packet, packet.Length, endpoint);
			} catch (SocketException exception) {
				Console.WriteLine(exception.ToString());
				// Error codes:
				// https://docs.microsoft.com/en-us/windows/win32/winsock/windows-sockets-error-codes-2
			}
		}
	}
}
