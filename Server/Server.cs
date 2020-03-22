using Microsoft.Xna.Framework;
using RunGun.Core;
using RunGun.Core.Networking;
using RunGun.Server.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using RunGun.Core.Utils;
using RunGun.Core.Game;
using System.Text;

namespace RunGun.Server
{

	#region jeff
	class NetworkPeer
	{

		public Guid ID;

		public IPEndPoint IPAddress { get; set; }

		public NetworkPeer(IPEndPoint endpoint) {
			IPAddress = endpoint;

			ID = Guid.NewGuid();
		}
	}

	class User : NetworkPeer
	{
		public string Nickname { get; set; }
		public double KeepAlive { get; set; }

		public User(IPEndPoint endpoint, string name) : base(endpoint) {
			Nickname = name;
		}
	}
	#endregion

	// TODO: make server occasionally talk shit in chat
	class Server {
		bool isRunning = true;
		float delta = 0;
		double networkRelayClock = 0;
		int iterator = 0;

		UdpListener udpListener;
		List<User> connectedUsers;
		GameWorld world;

		public Server(IPEndPoint endpoint) {
			Logging.Out("Loading plugins...");

			PluginManager.LoadPlugins();
			PluginManager.CallOnServerStart();
			PluginManager.CallOnPluginLoad();

			Logging.Out("Starting UDP Listener Thread...");
			udpListener = new UdpListener(endpoint);

			Logging.Out("Initializing gameworld...");
			world = new GameWorld();
			connectedUsers = new List<User>();

			world.levelGeometries = new List<LevelGeometry>() {
				new LevelGeometry(new Vector2(0, 20), new Vector2(20, 600), new Color(255, 0, 0)),
				new LevelGeometry(new Vector2(10, 500), new Vector2(1000, 40), new Color(0, 255, 128)),
				new LevelGeometry(new Vector2(400, 400), new Vector2(20, 50), new Color(128, 0, 128)),
			};
		}

		~Server() { }

		void GlobalMessage(string message, ConsoleColor color) {
			Logging.Out(message, color);
			SendToAll(P_ChatMessage(message));
		}

		#region Packet_Creation_Methods


		public static byte[] P_ConnectOK(Guid userGUID, int startFrame) {
			byte command = (byte)ServerCommand.CONNECT_OK;
			byte[] guid = userGUID.ToByteArray();
			byte[] frame = BitConverter.GetBytes(startFrame);

			return new byte[] { command,
				guid[0], guid[1], guid[2], guid[3],
				guid[4], guid[5], guid[6], guid[7],
				guid[8], guid[9], guid[10], guid[11],
				guid[12], guid[13], guid[14], guid[15],
				frame[0], frame[1], frame[2], frame[3]
			};
		}
		public static byte[] P_ConnectDeny(string reason) {
			//! if using non-ascii characters, need to change to UTF-8. (i.e regions with non-latin alphabets)
			var msg = Encoding.ASCII.GetBytes(reason);
			var b = new byte[msg.Length + 1];
			b[0] = (byte)ServerCommand.CONNECT_DENY;

			Array.Copy(msg, 0, b, 1, msg.Length);
			return b;
		}
		private static byte[] PacketGuid(ServerCommand command, Guid id) {
			byte cmd = (byte)command;
			byte[] guid = id.ToByteArray();
			return new byte[] { cmd, 
				guid[0], guid[1], guid[2], guid[3],
				guid[4], guid[5], guid[6], guid[7], 
				guid[8], guid[9], guid[10], guid[11], 
				guid[12], guid[13], guid[14], guid[15]
			};
		}
		public static byte[] P_UserJoined(Guid userGUID) {
			return PacketGuid(ServerCommand.USER_JOINED, userGUID);
		}
		public static byte[] P_UserLeft(Guid userGUID) {
			return PacketGuid(ServerCommand.USER_LEFT, userGUID);
		}
		public static byte[] P_ChatMessage(string message) {
			//! if using non-ascii characters, need to change to UTF-8. (i.e regions with non-latin alphabets)
			var msg = Encoding.ASCII.GetBytes(message);
			var b = new byte[msg.Length+1];
			b[0] = (byte)ServerCommand.CHAT_RELAY;

			Array.Copy(msg, 0, b, 1, msg.Length);
			return b;
		}
		public static byte[] P_SendMapData(LevelGeometry gm) {

			//int x, int y, int width, int height, byte r, byte g, byte b;
			byte[] bx = BitConverter.GetBytes((short)gm.Position.X); // conversion to short to save space.
			byte[] by = BitConverter.GetBytes((short)gm.Position.Y); // 2 bytes for short, 4 bytes for int
			byte[] bw = BitConverter.GetBytes((short)gm.Size.X); // you do the math, jackass.
			byte[] bh = BitConverter.GetBytes((short)gm.Size.Y);
			byte r = gm.Color.R;
			byte g = gm.Color.G;
			byte b = gm.Color.B;

			return new byte[] { (byte)ServerCommand.SEND_MAP_DATA,
				bx[0], bx[1], by[0], by[1],
				bw[0], bw[1], bh[0], bh[1],
				r, g, b
			};
		}
		public static byte[] P_ExistingUser() {
			return new byte[] { };
		}
		public static byte[] P_PlayerPos() {
			return new byte[] { };
		}
		public static byte[] P_Ping() {
			return new byte[] { };
		}
		public static byte[] P_PingReply() {
			return new byte[] {(byte)ServerCommand.PING_REPLY};
		}
		public static byte[] P_YourPid(short entityID) {
			byte[] id = BitConverter.GetBytes(entityID);
			return new byte[] {(byte)ServerCommand.YOUR_PID,
				id[0], id[1]
			};
		}//return String.Format("Player {0} {1} {2} {3} {4}", EntityID, UserNickname, color.R, color.G, color.B);
		public static byte[] P_AddEntity_Player(short entityID, byte r, byte g, byte b, string nickname) {
			byte[] id = BitConverter.GetBytes(entityID);
			var msg = Encoding.ASCII.GetBytes(nickname);
			var f = new byte[msg.Length + 6];
			f[0] = (byte)ServerCommand.ADD_E_PLAYER;
			f[1] = id[0];
			f[2] = id[1];
			f[3] = r;
			f[4] = g;
			f[5] = b;

			// ok that failed catastrophically.. LUL
			Array.Copy(msg, 0, f, 6, msg.Length);
			return f;
		}

		public static byte[] P_DeleteEntity(short entityID) {
			byte command = (byte)ServerCommand.DEL_ENTITY;
			byte[] id = BitConverter.GetBytes(entityID);
			return new byte[] { command, id[0], id[1]};
		}

		public static byte[] P_EntityPos(short entityID, int iter, float x, float y, float nextX, float nextY, float velX, float velY) {
			byte[] eb = BitConverter.GetBytes(entityID);
			byte[] ib = BitConverter.GetBytes(iter);
			byte[] xb = BitConverter.GetBytes(x);
			byte[] yb = BitConverter.GetBytes(y);
			byte[] nxb = BitConverter.GetBytes(nextX);
			byte[] nyb = BitConverter.GetBytes(nextY);
			byte[] vxb = BitConverter.GetBytes(velX);
			byte[] vyb = BitConverter.GetBytes(velY);

			return new byte[] {(byte)ServerCommand.ENTITY_POS, eb[0], eb[1], 
				ib[0], ib[1], ib[2], ib[3],
				xb[0], xb[1], xb[2], xb[3], 
				yb[0], yb[1], yb[2], yb[3],
				nxb[0], nxb[1], nxb[2], nxb[3],
				nyb[0], nyb[1], nyb[2], nyb[3],
				vxb[0], vxb[1], vxb[2], vxb[3],
				vyb[0], vyb[1], vyb[2], vyb[3]
			};
		}

		public static byte[] P_SendString(string message) {
			var msg = Encoding.ASCII.GetBytes(message);
			var b = new byte[msg.Length + 1];
			b[0] = (byte)ServerCommand.SEND_STRING;

			Array.Copy(msg, 0, b, 1, msg.Length);
			return b;
		}
		#endregion

		#region Client_Packet_Decoding_Methods

		void HandleDisconnect(User user, byte[] packet) {
			var player = GetPlayerOfUser(user);

			SendToAllExcept(user, P_UserLeft(user.ID));
			SendToAllExcept(user, P_DeleteEntity(player.EntityID));

			GlobalMessage(user.Nickname + " left.", ConsoleColor.Gray);
			world.RemoveEntity(player);
			connectedUsers.Remove(user);
		}

		void HandleSay(User user, byte[] packet) {
			// TODO: Sanitize string?
			string message = Encoding.ASCII.GetString(packet);
			GlobalMessage(user.Nickname + " : "+message, ConsoleColor.White);
		}
		void HandlePong(User user, byte[] packet) {

		}
		void HandlePing(User user, byte[] packet) {
			Send(user, P_PingReply());
			user.KeepAlive = 0;
		}
		void HandleConnect(NetworkPeer peer, byte[] packet) {

			string requestedNickname = Encoding.ASCII.GetString(packet);

			User user = new User(peer.IPAddress, requestedNickname);

			Player player = new Player() {
				UserGUID = user.ID,
				UserNickname = user.Nickname,
				EntityID = (short)world.entities.Count,
			};

			// notify join (before adding to clients)
			Send(user, P_ConnectOK(user.ID, world.physicsFrameIter));
			Send(user, P_YourPid(player.EntityID));

			world.AddEntity(player);

			foreach (var aPeer in connectedUsers) {
				Send(aPeer, P_AddEntity_Player(
					player.EntityID,
					player.color.R,
					player.color.G,
					player.color.B,
					player.UserNickname
				));
			}

			// TODO: change to map.entities
			foreach (Entity e in world.entities) {

				if (e is Player p) {
					Send(user, P_AddEntity_Player(
						p.EntityID,
						p.color.R,
						p.color.G,
						p.color.B,
						p.UserNickname
					));
				}
			}
			
			connectedUsers.Add(user);
			
			SendToAllExcept(user, P_UserJoined(user.ID));
			Send(user, P_ChatMessage("[Server] connected to " + GetServerIP() + ":"+GetServerPort()));
			GlobalMessage(user.Nickname + " joined.", ConsoleColor.Gray);

			foreach (LevelGeometry gm in world.levelGeometries) {
				Send(user, P_SendMapData(gm));
			}
		}

		public string GetServerIP() {
			return udpListener.GetServerIP();
		}

		public int GetServerPort() {
			return udpListener.GetServerPort();
		}

		void HandlePing(NetworkPeer peer, byte[] packet) {

		}
		void HandleGetOnlinePlayers(NetworkPeer peer, byte[] packet) {

		}

		void HandleGetServerName(NetworkPeer peer, byte[] packet) {

		}

		void HandleStartMoveLeft(User user) { GetPlayerOfUser(user).moveLeft = true; }
		void HandleStopMoveLeft(User user) { GetPlayerOfUser(user).moveLeft = false; }
		void HandleStartMoveRight(User user) { GetPlayerOfUser(user).moveRight = true; }
		void HandleStopMoveRight(User user) { GetPlayerOfUser(user).moveRight = false; }
		void HandleStartMoveJump(User user) { GetPlayerOfUser(user).moveJump = true; }
		void HandleStopMoveJump(User user) { GetPlayerOfUser(user).moveJump = false; }

		void HandleStartLookDown(User user) { GetPlayerOfUser(user).lookDown = true; }
		void HandleStopLookDown(User user) { GetPlayerOfUser(user).lookDown = false; }
		void HandleStartLookUp(User user) { GetPlayerOfUser(user).lookUp = true; }
		void HandleStopLookUp(User user) { GetPlayerOfUser(user).lookDown = false; }
		void HandleStartShoot(User user) { GetPlayerOfUser(user).shooting = true; }
		void HandleStopShoot(User user) { GetPlayerOfUser(user).shooting = false; }

		#endregion Network_Packet_Handling_Methods


		private void DecodePacket(Received received) {
			// cut 0th index off (the command ID)
			
			byte[] p = received.Packet;
	
			byte[] packet = new byte[p.Length - 1];
			Array.Copy(p, 1, packet, 0, p.Length - 1);
			byte netCommandID = p[0];

			ClientCommand command;
			try { command = (ClientCommand)netCommandID; } catch (Exception e) { return; }
			// failure condition: command was not a valid ClientCommand

			bool isClientConnected = IsUserConnected(received.Sender);

			if (isClientConnected) {
				User user = GetUser(received.Sender);
				switch (command) {
					case ClientCommand.CONNECT: // Client is already connected, tell them to fuck off maybe?
						Console.WriteLine("ALREADY CONNECTED DUMMY");
						break;
					case ClientCommand.DISCONNECT:
						HandleDisconnect(user, packet);
						break;
					case ClientCommand.CHAT_SAY:
						HandleSay(user, packet);
						break;
					case ClientCommand.PING:
						HandlePing(user, packet);
						break;
					case ClientCommand.MOVE_LEFT:
						HandleStartMoveLeft(user);
						break;
					case ClientCommand.MOVE_STOP_LEFT:
						HandleStopMoveLeft(user);
						break;
					case ClientCommand.MOVE_RIGHT:
						HandleStartMoveRight(user);
						break;
					case ClientCommand.MOVE_STOP_RIGHT:
						HandleStopMoveRight(user);
						break;
					case ClientCommand.MOVE_JUMP:
						HandleStartMoveJump(user);
						break;
					case ClientCommand.MOVE_STOP_JUMP:
						HandleStopMoveJump(user);
						break;
					case ClientCommand.LOOK_UP:
						HandleStartLookUp(user);
						break;
					case ClientCommand.STOP_LOOK_UP:
						HandleStopLookUp(user);
						break;
					case ClientCommand.LOOK_DOWN:
						HandleStartLookDown(user);
						break;
					case ClientCommand.STOP_LOOK_DOWN:
						HandleStopLookDown(user);
						break;
					case ClientCommand.SHOOT:
						HandleStartShoot(user);
						break;
					case ClientCommand.STOP_SHOOT:
						HandleStopShoot(user);
						break;
					default:
						// command was not listed...
						break;
				}
			} else {
				NetworkPeer peer = new NetworkPeer(received.Sender);
				switch (command) {
					case ClientCommand.CONNECT:
						HandleConnect(peer, packet);
						break;
					case ClientCommand.GET_ONLINE_PLAYERS:
						HandleGetOnlinePlayers(peer, packet);
						break;
					case ClientCommand.GET_SERVER_NAME:
						HandleGetServerName(peer, packet);
						break;
					case ClientCommand.PING:
						HandlePing(peer, packet);
						break;
					default:
						break;
				}
			}
		}

		private void ReadPacketQueue() {
			for (int i = 0; i < udpListener.GetCount(); i++) {
				DecodePacket(udpListener.Read());
			}
		}

		#region Get_User_Methods
		bool IsUserConnected(IPEndPoint endpoint) {
			foreach (var c in connectedUsers) {
				if (c.IPAddress.Equals(endpoint))
					return true;
			}
			return false;
		}
		User GetUser(IPEndPoint endpoint) {
			foreach (var c in connectedUsers) {
				if (c.IPAddress.Equals(endpoint))
					return c;
			}
			throw new ApplicationException("No user matching IPEndPoint found. Check with IsUserConnected first.");
		}
		User GetUser(string nickname) {
			foreach (var c in connectedUsers) {
				if (c.Nickname.Equals(nickname))
					return c;
			}
			throw new ApplicationException("No user matching IPEndPoint found. Check with IsUserConnected first.");
		}
		Player GetPlayerOfUser(User user) {
			foreach (var e in world.entities) {
				if (e is Player p) {
					if (p.UserGUID == user.ID) {
						return p;
					}
				}
			}
			throw new ApplicationException("No player associated with user. This is a serious error. Notify Josh.");
		}
		#endregion Get_User_Methods

		#region Message_Sendout_Methods
		public void SendToAll(byte[] packet) {
			foreach (var user in connectedUsers) {
				udpListener.Broadcast(user.IPAddress, packet);
			}
		}
		public void SendToAllExcept(User exception, byte[] packet) {
			foreach (var user in connectedUsers) {
				if (user != exception) {
					udpListener.Broadcast(user.IPAddress, packet);
				}
			}
		}
		public void Send(IPEndPoint endpoint, byte[] packet) {
			udpListener.Broadcast(endpoint, packet);
		}
		public void Send(NetworkPeer peer, byte[] packet) {
			Send(peer.IPAddress, packet);
		}
		public void Send(User user, byte[] packet) {
			Send(user.IPAddress, packet);
		}

		#endregion Message_Sendout_Methods

		private void UpdateKeepAlive(double dt) {
			// we copy ToArray so we can remove indices without crash
			// LUL
			foreach (var user in connectedUsers.ToArray()) {
				user.KeepAlive = user.KeepAlive + dt;
				if (user.KeepAlive > 10) {
					// TODO: client disconnect

					var player = GetPlayerOfUser(user);
					SendToAllExcept(user, P_UserLeft(user.ID));
					SendToAllExcept(user, P_DeleteEntity(player.EntityID));
					world.RemoveEntity(player);
					connectedUsers.Remove(user);
				}
			}
		}

		private void UpdateNetRelay(double dt) {
			networkRelayClock += dt;
			if (networkRelayClock > (1.0f / 30.0f)) {
				networkRelayClock = 0;
				
				foreach (var entity in world.entities) {
					if (entity is Player pe) {
						// TODO: see how many packets we can combine...
						SendToAll(P_EntityPos(pe.EntityID, iterator,
							pe.Position.X, pe.Position.Y,
							pe.NextPosition.X, pe.NextPosition.Y,
							pe.Velocity.X, pe.Velocity.Y));
					}
				}
			}
		}

		void Update(float dt) {
			ReadPacketQueue();
			UpdateNetRelay(dt);
			world.Update(dt);
			world.ServerUpdate(dt);
			UpdateKeepAlive(dt);
		}
		public int Run() {
			udpListener.Start();

			Stopwatch stopwatch = new Stopwatch();
			
			while (isRunning) {
				stopwatch = Stopwatch.StartNew();
				Update(delta);
				Thread.Sleep(8);
				delta = (float)stopwatch.Elapsed.TotalSeconds;
			}
			udpListener.Stop();
			stopwatch.Stop();
			return 0;
		}
	}
}
