using RunGun.Core;
using RunGun.Core.Bullshit;
using RunGun.Core.Networking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RunGun.Server.Networking
{

	class UnconnectedDatagramHook
	{
		private Dictionary<NetMsg, Action<Received, string[]>> callbacks = new Dictionary<NetMsg, Action<Received, string[]>>();

		public void Connect(NetMsg command, Action<Received, string[]> callback) {
			callbacks.Add(command, callback);
		}

		public void Call(Received sender, NetMsg command, string[] args) {
			foreach (KeyValuePair<NetMsg, Action<Received, string[]>> kvp in callbacks) {
				if (command == kvp.Key) {
					kvp.Value(sender, args);
					return;
				}
			}
		}

	}
	public class PlayerDatagramHook
	{
		private Dictionary<NetMsg, Action<Player, string[]>> callbacks = new Dictionary<NetMsg, Action<Player, string[]>>();

		public void Connect(NetMsg command, Action<Player, string[]> callback) {
			callbacks.Add(command, callback);
		}

		public void Call(Player sender, NetMsg command, string[] args) {
			foreach (KeyValuePair<NetMsg, Action<Player, string[]>> kvp in callbacks) {
				if (command == kvp.Key) {
					kvp.Value(sender, args);
					return;
				}
			}
		}
	}

	class ServerLayer
	{
		public UdpListener udpServer;

		Stack<Received> networkMessageStack;

		public PlayerDatagramHook OnPlayerPacket;
		public UnconnectedDatagramHook OnUnconnectedPacket;

		bool _runListenThread = true;

		public ServerLayer(IPEndPoint endpoint) {
			udpServer = new UdpListener(endpoint);

			OnPlayerPacket = new PlayerDatagramHook();
			OnUnconnectedPacket = new UnconnectedDatagramHook();

			networkMessageStack = new Stack<Received>();
		}


		public void StartNetworkThread() { // async void also works?
			_runListenThread = true;
			Task.Factory.StartNew(async () => {
				while (_runListenThread) {
					Received received = await udpServer.Receive();
					lock (networkMessageStack) {
						networkMessageStack.Push(received);
					}
				}
			});
		}

		public void StopNetworkThread() {
			_runListenThread = false;
		}

		void ProcessPacket(Received received) {
			string[] args = received.Message.Split(' ');

			NetMsg command;
			Enum.TryParse(args[0], true, out command);

			if (Server.IsClientConnected(received.Sender)) {
				var client = Server.GetClient(received.Sender);

				OnPlayerPacket.Call(Server.GetPlayer(client), command, args);

			} else {
				OnUnconnectedPacket.Call(received, command, args);
			}
		}

		public void ReadPacketQueue() {
			for (int i = 0; i < networkMessageStack.Count; i++) {
				var received = networkMessageStack.Pop();
				ProcessPacket(received);
			}
		}

		public void Send(string message, IPEndPoint endpoint) {
			udpServer.Reply(message, endpoint);
		}
	}
}
