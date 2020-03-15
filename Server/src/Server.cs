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
using NLua;
using System.IO;
using RunGun.Server.Utils;
using RunGun.Core.Utils;

namespace RunGun.Server
{
	class NetworkPeer
	{
		public IPEndPoint IPAddress { get; set; }

		public NetworkPeer(IPEndPoint endpoint) {
			IPAddress = endpoint;
		}
	}

	class User : NetworkPeer
	{

		public string Nickname { get; set; }
		public double KeepAlive { get; set; }
		public int ID { get; set; }

		public User(IPEndPoint endpoint, string name, int id) : base(endpoint) {
			Nickname = name;
			ID = id;
		}
	}

	// TODO: make server occasionally talk shit in chat
	class Server
	{
		// Lua VM shit
		Lua luaState = new Lua();

		// UDP Network Thread Bullshit
		UdpThread udpThread;

		bool isRunning = true;
		//GameServer server;

		public static int userIDCounter;

		static List<User> connectedUsers;
		List<LevelGeometry> geometry;
		static List<Player> players; // eventually this will be entities list
		double delta = 0;
		double networkRelayClock = 0;
		int iterator = 0;

		float physicsClock = 0;

		public Server(IPEndPoint endpoint) {

			udpThread = new UdpThread(endpoint);

			luaState.LoadCLRPackage();
			luaState.DoString(FileUtils.ReadEmbedded(LuaUtils.SCRIPT_SOURCE_DIR + ".test.lua"));

			string[] filenames = Directory.GetDirectories("plugins");

			// Plugin loading
			// TODO: sanity check this shit.
			foreach (var filename in filenames) {
				using (StreamReader stream = new StreamReader(filename + "/init.lua")) {
					string read = stream.ReadToEnd();

					luaState.DoString(read);
				}
			}

			connectedUsers = new List<User>();
			players = new List<Player>();

			geometry = new List<LevelGeometry>() {
				new LevelGeometry(new Vector2(0, 10), new Vector2(20, 400), new Color(1.0f, 0.0f, 0.0f)),
				new LevelGeometry(new Vector2(10, 420), new Vector2(800, 40), new Color(0.0f, 1.0f, 1.0f))
			};
		}

		~Server() {}
		//--------------------------------------------------------------------------------
		// 
		void HandleDisconnect(User user, string data) {
			
			connectedUsers.Remove(user);
			SendToAll(ServerCommand.USER_LEFT, user.ID.ToString());
			SendToAll(ServerCommand.CHAT_MSG, user.Nickname + " left.");
		}
		void HandleSay(User user, string data) {
			// TODO: Sanitize string?
			SendToAll(ServerCommand.CHAT_MSG, user.Nickname + " : " + data);
		}
		void HandlePong(User user, string data) {

		}
		void HandlePing(User user, string data) {
			Send(user, ServerCommand.PONG, "");
			user.KeepAlive = 0;
		}
		void HandleConnect(NetworkPeer peer, string data) {
			var args = data.Split(' ');

			string requestedNickname = args[0];

		/*	foreach (User usr in connectedUsers) {
				if (usr.Nickname == requestedNickname) {
					Send(peer, ServerCommand.CONNECT_DENY, "Nickname is already in use.");
					return;
				}
			}

			if (requestedNickname.Length < 4) {
				Send(peer, ServerCommand.CONNECT_DENY, "Nickname is too short. Must be At least 4 letters.");
				return;
			}*/

			userIDCounter++;

			User user = new User(peer.IPAddress, requestedNickname, userIDCounter);
			
			Player player = new Player(userIDCounter);
			// notify join (before adding to clients)
			Send(user, ServerCommand.CONNECT_OK, user.ID + " " + iterator);
			SendToAll(ServerCommand.USER_JOINED, user.ID.ToString());
			foreach (var aPeer in connectedUsers) {
				Send(user, ServerCommand.EXISTING_USER, aPeer.ID.ToString());
			}
			
			connectedUsers.Add(user);
			players.Add(player);

			Send(user, ServerCommand.CHAT_MSG, "[Server] connected to " + GetServerIP() + ":"+GetServerPort());

			SendToAll(ServerCommand.CHAT_MSG, user.Nickname + " joined.");

			// TODO: fix rEEEEEEEEEEEEEEEEEEE
			// sends map to client
			foreach (LevelGeometry gm in geometry) {
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
					case ClientCommand.PLR_LEFT:
						HandleStartMoveLeft(user);
						break;
					case ClientCommand.PLR_STOP_LEFT:
						HandleStopMoveLeft(user);
						break;
					case ClientCommand.PLR_RIGHT:
						HandleStartMoveRight(user);
						break;
					case ClientCommand.PLR_STOP_RIGHT:
						HandleStopMoveRight(user);
						break;
					case ClientCommand.PLR_JUMP:
						HandleStartMoveJump(user);
						break;
					case ClientCommand.PLR_STOP_JUMP:
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
			foreach (var p in players) {
				if (p.id == user.ID) {
					return p;
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

		void ProcessPhysics(Player e, float step) {
			e.Physics(step);

			e.isFalling = true;
			foreach (var geom in geometry) {
				CollisionSolver.SolveEntityAgainstGeometry(e, geom);
			}
		}

		void Physics(float step) {
			iterator++;

			foreach (var player in players) {
				ProcessPhysics(player, step);
			}
		}

		private void UpdateKeepAlive(double dt) {
			// we copy ToArray so we can remove indices without crash
			// LUL
			foreach (var user in connectedUsers.ToArray()) {
				user.KeepAlive = user.KeepAlive + dt;
				if (user.KeepAlive > 10) {
					// TODO: client disconnect
					players.Remove(GetPlayerOfUser(user));
					connectedUsers.Remove(user);

					SendToAll(ServerCommand.USER_LEFT, user.ID.ToString());
				}
			}
		}

		private void UpdateNetRelay(double dt) {
			networkRelayClock += dt;
			if (networkRelayClock > (1.0f / 30.0f)) {
				networkRelayClock = 0;

				foreach (var plr in players) {
					// TODO: see how many packets we can combine...
					string oppe = String.Format("{0} {1} {2} {3} {4} {5}",
						plr.id,
						plr.nextPosition.X, plr.nextPosition.Y,
						plr.velocity.X, plr.velocity.Y, iterator
					);
					SendToAll(ServerCommand.PLAYER_POS, oppe);
				}
			}
		}

		void Update(double dt) {
			ReadPacketQueue();
			UpdateNetRelay(dt);

			physicsClock += (float)delta;
			while (physicsClock > PhysicsProperties.PHYSICS_TIMESTEP) {
				physicsClock -= PhysicsProperties.PHYSICS_TIMESTEP;
				Physics(PhysicsProperties.PHYSICS_TIMESTEP);
			}

			foreach (var player in players)
				player.Update(dt);

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
