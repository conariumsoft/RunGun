using Microsoft.Xna.Framework;
using RunGun.Core;
using RunGun.Core.Bullshit;
using RunGun.Core.Networking;
using RunGun.Core.Physics;
using RunGun.Server.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RunGun.Server
{
	/* RunGun Server ToDo List
	 * load map from file.
	 * bullet wall penetration algorithm: f(x) = x * (1 / (t * d))
	 * 
	 * 
	 * 
	 * 
	 * 
	 * 
	 * 
	 */

	class Server
	{
		bool isRunning = true;
		ServerLayer serverLayer;
		public static int idAssignment;
		static List<Client> clients;
		List<LevelGeometry> geometry;
		static List<Player> players;
		double delta = 0;
		double networkRelayClock = 0;
		int iterator = 0;

		void OnConnect(Received received, string[] args) {
			// create client
			string nickname = args[1];

			Client client = new Client(received.Sender, nickname, idAssignment);
			Player player = new Player(idAssignment);


			idAssignment++;

			// notify join (before adding to clients)
			SendToAll(NetMsg.PEER_JOINED + " " + client.id);
			clients.Add(client);
			players.Add(player);
			SendToClient(client, NetMsg.CONNECT_ACK + " " + client.id + " " + iterator);

			// sends map to client
			foreach (LevelGeometry gm in geometry) {
				SendToClient(client, String.Format("{0} {1} {2} {3} {4} {5} {6} {7}", 
					NetMsg.DL_LEVEL_GEOMETRY, 
					gm.position.X, 
					gm.position.Y, 
					gm.size.X, 
					gm.size.Y, 
					gm.color.R,
					gm.color.G, 
					gm.color.B
				));
			}
		}

		void OnDisconnect(Player p, string[] args) {
			var client = GetClient(p);
			clients.Remove(client);
			SendToAll(NetMsg.PEER_LEFT + " " + client.id);
		}


		void OnPing(Player p, string[] args) {
			var client = GetClient(p);
			SendToClient(client, NetMsg.PONG + "");
			client.keepalive = 0;
		}

		void OnPong(Player p, string[] args) {
			 
		}

		void OnPlayerLeftDown  (Player p, string[] _) { p.moveLeft = true;   }
		void OnPlayerLeftUp    (Player p, string[] _) { p.moveLeft = false;  }
		void OnPlayerRightDown (Player p, string[] _) { p.moveRight = true;  }
		void OnPlayerRightUp   (Player p, string[] _) { p.moveRight = false; }
		void OnPlayerJumpDown  (Player p, string[] _) { p.moveJump = true;   }
		void OnPlayerJumpUp    (Player p, string[] _) { p.moveJump = false;  }

		public Server(IPEndPoint endpoint) {

			serverLayer = new ServerLayer(endpoint);

			serverLayer.OnUnconnectedPacket.Connect(NetMsg.CONNECT, OnConnect);

			serverLayer.OnPlayerPacket.Connect(NetMsg.PING, OnPing);
			serverLayer.OnPlayerPacket.Connect(NetMsg.PONG, OnPong);
			serverLayer.OnPlayerPacket.Connect(NetMsg.DISCONNECT, OnDisconnect);
			serverLayer.OnPlayerPacket.Connect(NetMsg.PLR_LEFT_DOWN,  OnPlayerLeftDown);
			serverLayer.OnPlayerPacket.Connect(NetMsg.PLR_LEFT_UP,    OnPlayerLeftUp);
			serverLayer.OnPlayerPacket.Connect(NetMsg.PLR_RIGHT_DOWN, OnPlayerRightDown);
			serverLayer.OnPlayerPacket.Connect(NetMsg.PLR_RIGHT_UP,   OnPlayerRightUp);
			serverLayer.OnPlayerPacket.Connect(NetMsg.PLR_JUMP_DOWN,  OnPlayerJumpDown);
			serverLayer.OnPlayerPacket.Connect(NetMsg.PLR_JUMP_UP,    OnPlayerJumpUp);
			
			clients = new List<Client>();
			players = new List<Player>();

			geometry = new List<LevelGeometry>() {
				new LevelGeometry(new Vector2(0, 10), new Vector2(20, 400), new Color(1, 0, 0)),
				new LevelGeometry(new Vector2(10, 420), new Vector2(800, 40), new Color(0, 1, 1))
			};
		}

		public static bool IsClientConnected(IPEndPoint endpoint) {
			foreach (var c in clients) {
				if (c.endpoint.Equals(endpoint))
					return true;
			}
			return false;
		}
		public static Client GetClient(IPEndPoint endpoint) {
			foreach (var c in clients) {
				if (c.endpoint.Equals(endpoint))
					return c;
			}
			return null;
		}
		public static Client GetClient(string nickname) {
			foreach (var c in clients) {
				if (c.nickname == nickname)
					return c;
			}
			return null;
		}
		public static Client GetClient(int id) {
			foreach (var c in clients) {
				if (c.id == id)
					return c;
			}
			return null;
		}
		public static Client GetClient(Player p) {
			foreach (var c in clients) {
				if (c.id == p.id)
					return c;
			}
			return null;
		}
		public static Player GetPlayer(Client c) {
			foreach(var plr in players) {
				if (plr.id == c.id) {
					return plr;
				}
			}
			return null; // shouldn't ever _actually_ return null. since if client exists, so does player.
		}
		public void SendToAll(string message) {
			foreach (var client in clients) {
				serverLayer.Send(message, client.endpoint);
			}
		}
		public void SendToAllExcept(Client exception, string message) {
			foreach (var client in clients) {
				if (client != exception) {
					serverLayer.Send(message, client.endpoint);
				}
			}
		}
		public void SendToClient(Client client, string message) {
			serverLayer.Send(message, client.endpoint);
		}

		void Update(double dt) {

			networkRelayClock += dt;

			if (networkRelayClock > (1.0f / 120.0f)) {
				networkRelayClock = 0;

				foreach (var plr in players) {
					// TODO: see how many packets we can combine...
					string oppe = String.Format("{0} {1} {2} {3} {4} {5} {6}",
						NetMsg.PLAYER_POS, plr.id,
						plr.nextPosition.X, plr.nextPosition.Y, 
						plr.velocity.X, plr.velocity.Y, iterator
					);
					SendToAll(oppe);
				}

			}

			foreach (var player in players)
				player.Update(dt);

			// gay hack?
			foreach (var client in clients.ToArray()) {

				client.keepalive = client.keepalive + dt;
				if (client.keepalive > 10) {
					// TODO: client disconnect
					players.Remove(GetPlayer(client));
					clients.Remove(client);

					SendToAll(NetMsg.PEER_LEFT +" "+ client.id);
				}
			}
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

		public int Run() {
			serverLayer.StartNetworkThread();

			Stopwatch stopwatch = new Stopwatch();
			float physicsClock = 0;

			while (isRunning) {
				stopwatch = Stopwatch.StartNew();

				serverLayer.ReadPacketQueue();

				physicsClock += (float)delta;
				while (physicsClock > PhysicsProperties.PHYSICS_TIMESTEP) {
					physicsClock -= PhysicsProperties.PHYSICS_TIMESTEP;
					Physics(PhysicsProperties.PHYSICS_TIMESTEP);
				}

				Update(delta);
				Thread.Sleep(8);
				delta = (stopwatch.Elapsed.TotalSeconds);
			}
			stopwatch.Stop();
			return 0;
		}
	}
}
