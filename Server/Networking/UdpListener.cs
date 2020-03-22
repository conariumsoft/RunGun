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

		Queue<Received> udpMessageQueue;

		bool isThreadRunning;
		//UdpListener udpServer;
		IPEndPoint IPAddress;

		private IPEndPoint ipAddress;

		public UdpListener(IPEndPoint endpoint) {
			udpMessageQueue = new Queue<Received>();
			ipAddress = endpoint;
			Client = new UdpClient(ipAddress);
		}

		public void Start() {
			isThreadRunning = true;
			Task.Factory.StartNew(async () => {
				while (isThreadRunning) {
					try {
						Received received = await Receive();
						lock (udpMessageQueue) {
							udpMessageQueue.Enqueue(received);
						}
					} catch (Exception e) {
						Console.WriteLine("SERVER NET ERR: " + e.Message);
					}
				}
			});
		}

		public string GetServerIP() {
			return IPAddress.Address.ToString();
		}

		public int GetServerPort() {
			return IPAddress.Port;
		}

		public void Stop() {
			isThreadRunning = false;
		}

		public Received Read() {
			lock (udpMessageQueue) {
				return udpMessageQueue.Dequeue();
			}
		}

		public int GetCount() {
			return udpMessageQueue.Count;
		}

		public void Broadcast(IPEndPoint endpoint, byte[] packet) {
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
