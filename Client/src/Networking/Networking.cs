using Microsoft.Xna.Framework;
using RunGun.Core.Networking;
using RunGun.Core.Utility;
using RunGun.Core.Utils;
using System;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace RunGun.Client.Networking
{
	public class PacketEvent : Event
	{
		//public NetMsg cmd;
		public List<string> args;

		public PacketEvent(List<string> arguments) {
			//cmd = command;
			args = arguments;
		}

		//public NetMsg GetCommand() { return cmd; }
		public List<string> GetArguments() { return args; }
	}

	class ClientArchitecture
	{

		UdpUser udpClient;

		Queue<Received> networkMessageQueue = new Queue<Received>();

		bool connected;
		bool connecting;
		bool isListenThreadRunning = true;

		public event Action<int, int> OnConnectAccept;
		public event Action<string> OnConnectDenied; // string denyReason
		public event Action<string> OnKicked;
		public event Action<int> OnPeerJoined;
		public event Action<int> OnPeerLeft;
		public event Action OnPing;
		public event Action<Vector2, Vector2, Color> OnReceiveMapData;
		public event Action<int, Vector2, Vector2, int> OnPlayerPosition;
		public event Action<string> OnChatMessage;
		public event Action OnPong;
		public event Action<int> OnExistingPeer;

		public ClientArchitecture() {
			

		}

		public void Connect(string ip, int port, string nickname) {
			Console.WriteLine("Begin Conn");
			udpClient = UdpUser.Connect(ip, port);
			
			connecting = true;
			Task.Factory.StartNew(async () => {
				while (isListenThreadRunning) {
					try {
						var received = await udpClient.Receive();
						//Console.WriteLine("Got One!");
						lock (networkMessageQueue) {
							networkMessageQueue.Enqueue(received);
						}
					} catch (Exception ex) {
						Console.WriteLine("CLIENT NETWORK ERR: " + ex.Message + " " +ex.Source + " "+ex.StackTrace);
					}
				}
			});
			udpClient.Send(String.Format("{0} {1}", (int)ClientCommand.CONNECT, nickname));
		}

		public void Disconnect() {
			isListenThreadRunning = false;
			connected = false;
			connecting = false;
		}

		public void Send(ClientCommand command) {
			udpClient.Send((int)command + "");
		}
		public void Send(ClientCommand command, string message) {
			udpClient.Send((int)command + " " + message);
		}

		public void ConnectFromCurrentServer(string ip, int port, string nickname, string data) { }

		public bool IsConnected() {
			return connected;
			// Mein Herz Brennt
		}

		private void HandleConnectOK(string data) {
			var args = data.Split(' ');
			int clientAssignedID;
			int serverPhysicsFrame;

			if (args.Length < 2) return;
			if (!int.TryParse(args[0], out clientAssignedID)) return;
			if (!int.TryParse(args[1], out serverPhysicsFrame)) return;
			
			OnConnectAccept?.Invoke(clientAssignedID, serverPhysicsFrame);
		}

		private void HandleConnectDeny(string data) {

			OnConnectDenied?.Invoke(data);
		}
		private void HandlePong(string data) {
			OnPong?.Invoke();
		}
		private void HandlePlayerPosition(string data) {
			var args = data.Split(' ');
			int id;
			float x;
			float y;
			float vx;
			float vy;
			int stepIter;

			if (!int.TryParse(args[0], out id)) return;
			if (!float.TryParse(args[1], out x)) return;
			if (!float.TryParse(args[2], out y)) return;
			if (!float.TryParse(args[3], out vx)) return;
			if (!float.TryParse(args[4], out vy)) return;
			if (!int.TryParse(args[5], out stepIter)) return;

			OnPlayerPosition?.Invoke(id, new Vector2(x, y), new Vector2(vx, vy), stepIter);
		}

		private void HandleKick(string data) {
			connected = false;
			OnKicked?.Invoke(data);
		}

		private void HandleChatMessage(string data) {

			OnChatMessage?.Invoke(data);
		}

		private void HandleExistingUser(string data) {
			var args = data.Split(' ');
			int peerID;

			if (!int.TryParse(args[0], out peerID)) return;

			OnExistingPeer?.Invoke(peerID);
		}
		private void HandlePeerLeft(string data) {
			var args = data.Split(' ');
			int peerID;

			if (!int.TryParse(args[0], out peerID)) return;
			OnPeerLeft?.Invoke(peerID);
		}

		private void HandlePeerJoined(string data) {
			var args = data.Split(' ');
			int newPeerID;

			if (!int.TryParse(args[0], out newPeerID)) return;
			OnPeerJoined?.Invoke(newPeerID);
		}
		private void HandleGetMapData(string data) {
			var args = data.Split(' ');
			int x, y, w, h;
			int r, g, b;

			if (!int.TryParse(args[0], out x)) return;
			if (!int.TryParse(args[1], out y)) return;
			if (!int.TryParse(args[2], out w)) return;
			if (!int.TryParse(args[3], out h)) return;
			if (!int.TryParse(args[4], out r)) return;
			if (!int.TryParse(args[5], out g)) return;
			if (!int.TryParse(args[6], out b)) return;

			OnReceiveMapData?.Invoke(new Vector2(x, y), new Vector2(w, h), new Color(r, g, b));
		}

		private void ProcessPacket(Received received) {
			string message = received.Message;
			string netCommandIDAsString = StringUtils.ReadUntil(message, ' ');
			ServerCommand command;
			bool success = Enum.TryParse(netCommandIDAsString, out command);

			if (!success)
				return;

			string data = StringUtils.ReadAfter(message, ' ');

			switch(command) {
				case ServerCommand.CONNECT_OK:
					HandleConnectOK(data);
					break;
				case ServerCommand.CONNECT_DENY:
					HandleConnectDeny(data);
					break;
				case ServerCommand.KICK:
					HandleKick(data);
					break;
				case ServerCommand.CHAT_MSG:
					HandleChatMessage(data);
					break;
				case ServerCommand.EXISTING_USER:
					HandleExistingUser(data);
					break;
				case ServerCommand.USER_JOINED:
					HandlePeerJoined(data);
					break;
				case ServerCommand.USER_LEFT:
					HandlePeerLeft(data);
					break;
				case ServerCommand.SEND_MAP_DATA:
					HandleGetMapData(data);
					break;
				case ServerCommand.PLAYER_POS:
					HandlePlayerPosition(data);
					break;
				case ServerCommand.PONG:
					HandlePong(data);
					break;
				default:
					break;
			}

			if (command == ServerCommand.CONNECT_OK) {
				// successful connection
				connected = true;
				connecting = false;
			} else if (command == ServerCommand.CONNECT_DENY) {
				// unsuccessful connection
				connecting = false;
			} else if (command == ServerCommand.KICK) {
				// disconnected from the server
				connected = false;
				connecting = false;
			}
		}

		private void ReadPacketQueue() {
			for (int i = 0; i < networkMessageQueue.Count; i++) {
				var received = networkMessageQueue.Dequeue();
				ProcessPacket(received);
			}
		}

		public void Update(double dt) {
			ReadPacketQueue();
			
		}
	}
}
