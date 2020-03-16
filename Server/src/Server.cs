using Microsoft.Xna.Framework;
using RunGun.Core;
using RunGun.Core.Networking;
using RunGun.Core.Physics;
using RunGun.Server.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using RunGun.Core.Utils;

namespace RunGun.Server
{
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

	// TODO: make server occasionally talk shit in chat
	class Server
	{

		bool isRunning = true;
		double delta = 0;
		double networkRelayClock = 0;
		int iterator = 0;

		UdpThread udpThread;
		List<User> connectedUsers;
		GameWorld world;

		public Server(IPEndPoint endpoint) {
			Logging.Out("Loading plugins...");

			PluginManager.LoadPlugins();

			Logging.Out("Starting UDP Listener Thread...");
			udpThread = new UdpThread(endpoint);

			Logging.Out("Initializing gameworld...");
			world = new GameWorld();
			connectedUsers = new List<User>();

			world.levelGeometries = new List<LevelGeometry>() {
				new LevelGeometry(new Vector2(0, 10), new Vector2(20, 400), new Color(1.0f, 0.0f, 0.0f)),
				new LevelGeometry(new Vector2(10, 420), new Vector2(800, 40), new Color(0.0f, 1.0f, 1.0f))
			};
		}

		~Server() {}

		//--------------------------------------------------------------------------------
		// 
		void GlobalMessage(string message, ConsoleColor color) {
			Logging.Out(message, color);
			SendToAll(ServerCommand.CHAT_MSG, message);
		}

		void HandleDisconnect(User user, string data) {
			var player = GetPlayerOfUser(user);
			SendToAllExcept(user, ServerCommand.USER_LEFT, user.ID.ToString());
			SendToAllExcept(user, ServerCommand.DEL_ENTITY, player.EntityID.ToString());
			GlobalMessage(user.Nickname + " left.", ConsoleColor.Gray);
			world.RemoveEntity(player);
			connectedUsers.Remove(user);
		}

		void HandleSay(User user, string data) {
			// TODO: Sanitize string?
			GlobalMessage(user.Nickname + " : "+data, ConsoleColor.White);
		}

		void HandlePong(User user, string data) {

		}
		void HandlePing(User user, string data) {
			Send(user, ServerCommand.PING_REPLY, "");
			user.KeepAlive = 0;
		}
		void HandleConnect(NetworkPeer peer, string data) {
			var args = data.Split(' ');

			string requestedNickname = args[0];

			/*foreach (User usr in connectedUsers) {
				if (usr.Nickname == requestedNickname) {
					Send(peer, ServerCommand.CONNECT_DENY, "Nickname is already in use.");
					return;
				}
			}

			if (requestedNickname.Length < 4) {
				Send(peer, ServerCommand.CONNECT_DENY, "Nickname is too short. Must be At least 4 letters.");
				return;
			}*/

			User user = new User(peer.IPAddress, requestedNickname);
			
			Player player = new Player();
			player.UserGUID = user.ID;
			// notify join (before adding to clients)
			Send(user, ServerCommand.CONNECT_OK, user.ID.ToString() + " "+ world.physicsFrameIter);
			SendToAllExcept(user, ServerCommand.USER_JOINED, user.ID.ToString());

			world.AddEntity(player);
			foreach (var aPeer in connectedUsers) {
				Send(aPeer, ServerCommand.ADD_ENTITY, "Player " + player.EntityID);
			}

			// TODO: change to map.entities
			foreach (Entity e in world.entities) {
				Send(user, ServerCommand.ADD_ENTITY, "Player " + e.EntityID);
			}

			connectedUsers.Add(user);

			Send(user, ServerCommand.YOUR_PID, player.EntityID.ToString());
			Send(user, ServerCommand.CHAT_MSG, "[Server] connected to " + GetServerIP() + ":"+GetServerPort());
			GlobalMessage(user.Nickname + " joined.", ConsoleColor.Gray);

			foreach (LevelGeometry gm in world.levelGeometries) {
				Send(user, ServerCommand.SEND_MAP_DATA, gm.Serialize());
			}
		}

		public string GetServerIP() {
			return udpThread.GetServerIP();
		}

		public int GetServerPort() {
			return udpThread.GetServerPort();
		}

		void HandlePing(NetworkPeer peer, string data) {

		}
		void HandleGetOnlinePlayers(NetworkPeer peer, string data) {

		}

		void HandleGetServerName(NetworkPeer peer, string data) {

		}

		void HandleStartMoveLeft(User user) { GetPlayerOfUser(user).moveLeft = true; }
		void HandleStopMoveLeft(User user) { GetPlayerOfUser(user).moveLeft = false; }
		void HandleStartMoveRight(User user) { GetPlayerOfUser(user).moveRight = true; }
		void HandleStopMoveRight(User user) { GetPlayerOfUser(user).moveRight = false; }
		void HandleStartMoveJump(User user) { GetPlayerOfUser(user).moveJump = true; }
		void HandleStopMoveJump(User user) { GetPlayerOfUser(user).moveJump = false; }

		//--------------------------------------------------------------------------------

		private void DecodePacket(Received received) {

			string message = received.Message;
			string netCommandIDAsString = StringUtils.ReadUntil(message, ' ');
			int netCommandIDAsInt;
			bool success = int.TryParse(netCommandIDAsString, out netCommandIDAsInt);

			if (!success) // TODO: bitch about the packet being useless shit
				return; // failure condition: netCommandID was not a valid integer...

			ClientCommand command;
			try { command = (ClientCommand)netCommandIDAsInt; } catch (Exception e) { return; }
			// failure condition: command was not a valid ClientCommand

			string data = StringUtils.ReadAfter(message, ' ');
			bool isClientConnected = IsUserConnected(received.Sender);

			if (isClientConnected) {
				User user = GetUser(received.Sender);
				switch (command) {
					case ClientCommand.CONNECT: // Client is already connected, tell them to fuck off maybe?

						break;
					case ClientCommand.DISCONNECT:
						HandleDisconnect(user, data);
						break;
					case ClientCommand.SAY:
						HandleSay(user, data);
						break;
					case ClientCommand.PING:
						HandlePing(user, data);
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
					default:
						// command was not listed...
						break;
				}
			} else {
				NetworkPeer peer = new NetworkPeer(received.Sender);
				switch (command) {
					case ClientCommand.CONNECT:
						HandleConnect(peer, data);
						break;
					case ClientCommand.GET_ONLINE_PLAYERS:
						HandleGetOnlinePlayers(peer, data);
						break;
					case ClientCommand.GET_SERVER_NAME:
						HandleGetServerName(peer, data);
						break;
					case ClientCommand.PING:
						HandlePing(peer, data);
						break;
					default:
						break;
				}
			}
		}

		private void ReadPacketQueue() {
			for (int i = 0; i < udpThread.GetCount(); i++) {
				DecodePacket(udpThread.Read());
			}
		}

		//---------------------------------------------------------------
		// Utility methods
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

		public void SendToAll(ServerCommand command, string message) {
			foreach (var user in connectedUsers) {
				udpThread.Broadcast(user.IPAddress, command, message);
			}
		}
		public void SendToAllExcept(User exception, ServerCommand command, string message) {
			foreach (var user in connectedUsers) {
				if (user != exception) {
					udpThread.Broadcast(user.IPAddress, command, message);
				}
			}
		}
		public void Send(IPEndPoint endpoint, ServerCommand code, string data) {
			udpThread.Broadcast(endpoint, code, data);
		}
		public void Send(NetworkPeer peer, ServerCommand code, string data) {
			Send(peer.IPAddress, code, data);
		}
		public void Send(User user, ServerCommand code, string message) {
			Send(user.IPAddress, code, message);
		}

		private void UpdateKeepAlive(double dt) {
			// we copy ToArray so we can remove indices without crash
			// LUL
			foreach (var user in connectedUsers.ToArray()) {
				user.KeepAlive = user.KeepAlive + dt;
				if (user.KeepAlive > 10) {
					// TODO: client disconnect

					var player = GetPlayerOfUser(user);
					SendToAllExcept(user, ServerCommand.USER_LEFT, user.ID.ToString());
					SendToAllExcept(user, ServerCommand.DEL_ENTITY, player.EntityID.ToString());
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
					if (entity is PhysicalEntity pe) {
						// TODO: see how many packets we can combine...
						string packet = String.Format("{0} {1} {2} {3} {4} {5} {6} {7}",
							pe.EntityID, iterator,
							pe.position.X, pe.position.Y,
							pe.nextPosition.X, pe.nextPosition.Y,
							pe.velocity.X, pe.velocity.Y
						);
						SendToAll(ServerCommand.ENTITY_POS, packet);
					}
				}
			}
		}

		void Update(double dt) {
			ReadPacketQueue();
			UpdateNetRelay(dt);

			world.Update(delta);

			UpdateKeepAlive(dt);
		}
		public int Run() {
			udpThread.Start();

			Stopwatch stopwatch = new Stopwatch();
			
			while (isRunning) {
				stopwatch = Stopwatch.StartNew();
				Update(delta);
				Thread.Sleep(0);
				delta = (stopwatch.Elapsed.TotalSeconds);
			}
			udpThread.Stop();
			stopwatch.Stop();
			return 0;
		}
	}
}
