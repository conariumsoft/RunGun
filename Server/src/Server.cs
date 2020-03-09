using Microsoft.Xna.Framework;
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
	class Server
	{

		UdpListener udpServer;
		List<Client> clients;
		Stack<Received> networkMessageStack;
		List<LevelGeometry> geometry;
		bool killed = false;
		double delta = 0;
		double networkRelayClock = 0;

		public Server(IPEndPoint endpoint) {
			udpServer = new UdpListener(endpoint);
			clients = new List<Client>();
			networkMessageStack = new Stack<Received>();
			geometry = new List<LevelGeometry>();

			geometry.Add(new LevelGeometry(new Vector2(0, 10), new Vector2(20, 200)));
			geometry.Add(new LevelGeometry(new Vector2(10, 250), new Vector2(300, 40)));
		}

		public bool IsClientConnected(IPEndPoint endpoint) {
			foreach (var c in clients) {

				if (c.endpoint.Equals(endpoint)) {
					return true;
				}
			}
			return false;
		}

		public Client GetClient(IPEndPoint endpoint) {
			foreach (var c in clients) {

				if (c.endpoint.Equals(endpoint)) {
					return c;
				}
			}
			return null;
		}

		public Client GetClient(string nickname) {
			foreach (var c in clients) {

				if (c.nickname == nickname) {
					return c;
				}
			}
			return null;
		}

		public Client GetClient(int id) {
			foreach (var c in clients) {

				if (c.id == id) {
					return c;
				}
			}
			return null;
		}

		public void SendToAll(string message) {
			foreach (var client in clients) {
				udpServer.Reply(message, client.endpoint);
			}
		}

		public void SendToAllExcept(Client exception, string message) {
			foreach (var client in clients) {
				if (client != exception) {
					udpServer.Reply(message, client.endpoint);
				}
			}
		}

		public void SendToClient(Client client, string message) {
			udpServer.Reply(message, client.endpoint);
		}

		void Update(double dt) {

			networkRelayClock += dt;

			if (networkRelayClock > (1.0f / 30.0f)) {
				networkRelayClock = 0;
				foreach (var client in clients)
					SendToClient(client, String.Format("you {0} {1}", client.character.Position.X, client.character.Position.Y));
			}

			foreach (var client in clients)
				client.character.Update(dt);


			// gay hack?
			foreach (var client in clients.ToArray()) {

				client.keepalive = client.keepalive + dt;
				if (client.keepalive > 10) {
					// TODO: client disconnect
					clients.Remove(client);
					SendToAll("left " + client.nickname);
				}
			}
		}

		void Physics(float step) {
			foreach (var client in clients) {

				client.character.Physics(step);

				foreach (var geom in geometry) {

					bool result = CollisionSolver.CheckAABB(client.character.NextPosition, client.character.BoundingBox, geom.GetCenter(), geom.GetDimensions());

					if (result) {
						var separation = CollisionSolver.GetSeparationAABB(client.character.NextPosition, client.character.BoundingBox, geom.GetCenter(), geom.GetDimensions());
						var normal = CollisionSolver.GetNormalAABB(separation, client.character.Velocity);

						client.character.NextPosition += separation;

					}
				}
			}
		}

		void HandleConnectAttempt(Received data, string[] words) {
			string nickname = words[1];
			var client = new Client(data.Sender, nickname);
			SendToAll("join " + client.nickname);
			clients.Add(client);

			// sends map to client
			foreach (LevelGeometry gm in geometry) {
				SendToClient(client, String.Format("geom {0} {1} {2} {3}", gm.Position.X, gm.Position.Y, gm.size.X, gm.size.Y));
			}
		}

		void HandleDisconnect(Client client) {
			clients.Remove(client);
			SendToAll("left " + client.nickname);
		}

		void HandleChat() { }

		void HandlePing(Client client) {
			SendToClient(client, "pong");
			//udpServer.Reply("pong", received.Sender);
			client.keepalive = 0;
		}


		void HandleNetworkMessage(Received received) {
			string[] words = received.Message.Split(' ');

			if (words[0] == "connect")
				HandleConnectAttempt(received, words);

			// no other messages should be accepted from not-connected
			if (!IsClientConnected(received.Sender))
				return;

			var client = GetClient(received.Sender);

			switch (words[0]) {
				case "disconnect":
					HandleDisconnect(client);
					break;
				case "chat":
					HandleChat();
					break;
				case "ping":
					HandlePing(client);
					break;
				case "move_left_start":
					client.character.MoveLeft = true;
					break;
				case "move_left_end":
					client.character.MoveLeft = false;
					break;
				case "move_right_start":
					client.character.MoveRight = true;
					break;
				case "move_right_end":
					client.character.MoveRight = false;
					break;
				case "move_jump_start":
					client.character.MoveJump = true;
					break;
				case "move_jump_end":
					client.character.MoveJump = false;
					break;
				default:
					break;
			}
		}

		void HandleNetworkStack() {
			lock (networkMessageStack) {
				for (int i = 0; i < networkMessageStack.Count; i++) {
					Received received = networkMessageStack.Pop();
					
					HandleNetworkMessage(received);
				}
			}
		}

		public void StartNetworkThread() {
			Task.Factory.StartNew(async () => {
				while (true) {
					Received received = await udpServer.Receive();
					lock (networkMessageStack) {
						networkMessageStack.Push(received);
					}
				}
			});
		}

		public int Run() {
			StartNetworkThread();

			Stopwatch stopwatch = new Stopwatch();
			float physicsClock = 0;

			while (!killed) {
				stopwatch = Stopwatch.StartNew();

				HandleNetworkStack();

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
