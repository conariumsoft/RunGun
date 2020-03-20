using Microsoft.Xna.Framework;
using RunGun.Core.Game;
using RunGun.Core.Networking;
using RunGun.Core.Utility;
using RunGun.Core.Utils;
using System;
using System.Collections.Generic;
using System.Text;
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

		public event Action<Guid, int> OnConnectAccept;
		public event Action<string> OnConnectDenied; // string denyReason
		public event Action<string> OnKicked;
		//public event Action<int> OnPeerJoined;
		//public event Action<int> OnPeerLeft;
		public event Action OnPing;
		public event Action<Vector2, Vector2, Color> OnReceiveMapData;
		public event Action<string> OnChatMessage;
		public event Action OnPingReply;
		//public event Action<int> OnExistingPeer;
		public event Action<Entity> OnAddEntity;
		public event Action<short> OnDeleteEntity;
		public event Action<short> OnGetLocalPlayerID;
		public event Action<short, int, Vector2, Vector2, Vector2> OnEntityPosition;

		public ClientArchitecture() {
			
		}

		private static byte[] P_Connect(string nickname) {
			var msg = Encoding.ASCII.GetBytes(nickname);
			var b = new byte[msg.Length + 1];
			b[0] = (byte)ClientCommand.CONNECT;

			Array.Copy(msg, 0, b, 1, msg.Length);
			return b;
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
			Send(P_Connect(nickname));
		}

		public void Disconnect() {
			isListenThreadRunning = false;
			connected = false;
			connecting = false;
		}

		public void Send(byte[] packet) {
			udpClient.Send(packet);
		}

		public void ConnectFromCurrentServer(string ip, int port, string nickname, string data) { }

		public bool IsConnected() {
			return connected;
			// Mein Herz Brennt
		}

		//------------------------------------------------------------------------------------------//
		
		private static Guid GetGuid(byte[] arr, int start) {
			byte[] data = new byte[16];

			for (int i = 0; i < 16; i++) {
				data[i] = arr[i + start];
			}
			return new Guid(data);
		}
		
		private void HandleConnectOK(byte[] packet) {

			// TODO: error checking
			Guid ourID = GetGuid(packet, 0);
			int serverPhysicsFrame = BitConverter.ToInt32(packet, 16);
			
			OnConnectAccept?.Invoke(ourID, serverPhysicsFrame);
		}

		private void HandleConnectDeny(byte[] packet) {
			string message = BitConverter.ToString(packet);
			OnConnectDenied?.Invoke(message);
		}

		private void HandlePing(byte[] packet) {
			OnPing?.Invoke();
		}
		private void HandlePingReply(byte[] packet) {
			OnPingReply?.Invoke();
		}

		private void HandleAddEntity_Player(byte[] packet) {
			var player = Player.Decode(packet);

			if (player != null) {
				OnAddEntity?.Invoke(player);
			}
			return;
		} 
		private void HandleDeleteEntity(byte[] packet) {

			short entityID = BitConverter.ToInt16(packet, 0);

			OnDeleteEntity?.Invoke(entityID);
		}

		private void HandleYourPID(byte[] packet) {
			short pid = BitConverter.ToInt16(packet, 0);

			OnGetLocalPlayerID?.Invoke(pid);
		}
		private void HandleEntityPosition(byte[] packet) {
			short id = BitConverter.ToInt16(packet, 0);
			int iter = BitConverter.ToInt32(packet, 2);
			float x = BitConverter.ToSingle(packet, 6);
			float y = BitConverter.ToSingle(packet, 10);
			float nx = BitConverter.ToSingle(packet, 14);
			float ny = BitConverter.ToSingle(packet, 18);
			float vx = BitConverter.ToSingle(packet, 22);
			float vy = BitConverter.ToSingle(packet, 26);

			OnEntityPosition?.Invoke(id, iter, new Vector2(x, y), new Vector2(nx, ny), new Vector2(vx, vy));
		}

		private void HandleKick(byte[] packet) {
			connected = false;
			string message = Encoding.ASCII.GetString(packet);
			OnKicked?.Invoke(message);
		}

		private void HandleChatMessage(byte[] packet) {
			string message = Encoding.ASCII.GetString(packet);
			OnChatMessage?.Invoke(message);
		}

		private void HandleGetMapData(byte[] packet) {
			short x = BitConverter.ToInt16(packet, 0);
			short y = BitConverter.ToInt16(packet, 2);
			short w = BitConverter.ToInt16(packet, 4);
			short h = BitConverter.ToInt16(packet, 6);
			byte r = packet[7];
			byte g = packet[8];
			byte b = packet[9];

			OnReceiveMapData?.Invoke(new Vector2(x, y), new Vector2(w, h), new Color(r, g, b, (byte)0));
		}

		private void ProcessPacket(Received received) {

			// cut 0th index off (the command ID)
			byte[] p = received.Packet;
			byte[] packet = new byte[p.Length - 1];
			Array.Copy(p, 1, packet, 0, p.Length - 1);

			ServerCommand command;
			try {
				command = (ServerCommand)p[0];
			}catch (Exception e) {
				// fuck;
				return;
			}

			switch(command) {
				case ServerCommand.CONNECT_OK:
					HandleConnectOK(packet);
					break;
				case ServerCommand.CONNECT_DENY:
					HandleConnectDeny(packet);
					break;
				case ServerCommand.KICK:
					HandleKick(packet);
					break;
				case ServerCommand.CHAT_MSG:
					HandleChatMessage(packet);
					break;
				case ServerCommand.EXISTING_USER:
					//HandleExistingUser(packet);
					break;
				case ServerCommand.USER_JOINED:
					//HandlePeerJoined(packet);
					break;
				case ServerCommand.USER_LEFT:
					//HandlePeerLeft(packet);
					break;
				case ServerCommand.SEND_MAP_DATA:
					HandleGetMapData(packet);
					break;
				case ServerCommand.PLAYER_POS:
					//HandlePlayerPosition(packet);
					break;
				case ServerCommand.PING:
					HandlePing(packet);
					break;
				case ServerCommand.PING_REPLY:
					HandlePingReply(packet);
					break;
				case ServerCommand.YOUR_PID:
					HandleYourPID(packet);
					break;
				case ServerCommand.ADD_E_PLAYER:
					HandleAddEntity_Player(packet);
					break;
				case ServerCommand.DEL_ENTITY:
					HandleDeleteEntity(packet);
					break;
				case ServerCommand.ENTITY_POS:
					HandleEntityPosition(packet);
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
