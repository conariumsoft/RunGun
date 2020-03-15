using RunGun.Core;
using RunGun.Core.Networking;
using RunGun.Core.Utility;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RunGun.Server.Networking
{
	class UdpThread
	{

		Queue<Received> udpMessageQueue;

		bool isThreadRunning;
		UdpListener udpServer;
		IPEndPoint IPAddress;

		public UdpThread(IPEndPoint endpoint) {
			udpServer = new UdpListener(endpoint);
			udpMessageQueue = new Queue<Received>();
			IPAddress = endpoint;
		}

		public void Start() {
			isThreadRunning = true;
			Task.Factory.StartNew(async () => {
				while (isThreadRunning) {
					try {
						Received received = await udpServer.Receive();
						lock (udpMessageQueue) {
							udpMessageQueue.Enqueue(received);
						}
					} catch(Exception e) {
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

		public void Broadcast(IPEndPoint endpoint, ServerCommand code, string data) {
			udpServer.Reply(code + " " + data, endpoint);
		}
	}
}
