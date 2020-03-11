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
using System.Threading.Tasks;

namespace RunGun.Server
{
	/* RunGun Server ToDo List
	 * load map from file.
	 * 
	 * 
	 */
	class Server
	{

		public static int idAssignment;

		UdpListener udpServer;
		List<Client> clients;
		Stack<Received> networkMessageStack;
		List<LevelGeometry> geometry;
		bool killed = false;
		double delta = 0;
		double networkRelayClock = 0;

		int iterator = 0;

		public Server(IPEndPoint endpoint) {

			udpServer = new UdpListener(endpoint);
			clients = new List<Client>();
			networkMessageStack = new Stack<Received>();
			geometry = new List<LevelGeometry>();

			geometry.Add(new LevelGeometry(new Vector2(0, 10), new Vector2(20, 400), new Color(1, 0, 0)));
			geometry.Add(new LevelGeometry(new Vector2(10, 420), new Vector2(800, 40), new Color(0, 1, 1)));
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

			if (networkRelayClock > (1.0f / 120.0f)) {
				networkRelayClock = 0;
				foreach (var client in clients) {
					string oppe = String.Format("{0} {1} {2} {3} {4} {5} {6}", NetMsg.PLAYER_POS, client.character.id, client.character.nextPosition.X, client.character.nextPosition.Y, client.character.velocity.X, client.character.velocity.Y, iterator);
					SendToAll(oppe);
				}
			}

			foreach (var client in clients)
				client.character.Update(dt);

			// gay hack?
			foreach (var client in clients.ToArray()) {

				client.keepalive = client.keepalive + dt;
				if (client.keepalive > 10) {
					// TODO: client disconnect
					clients.Remove(client);
					SendToAll(NetMsg.PEER_LEFT +" "+ client.character.id);
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

			foreach (var client in clients) {
				ProcessPhysics(client.character, step);
			}
		}

		void HandleConnectAttempt(Received data, string[] words) {
			Console.WriteLine("Client conn");
			string nickname = words[1];
			var client = new Client(data.Sender, nickname);
			client.character.id = idAssignment;
			idAssignment++;
			SendToAll(NetMsg.PEER_JOINED +" "+ client.character.id);

			// lol wtf
			//foreach (var cli in clients) {
				//SendToClient(client, NetMsg.PEER_JOINED+" "+cli.character.id);
			//}

			clients.Add(client);
			SendToClient(client, NetMsg.CONNECT_ACK + " " + client.character.id + " "+iterator);


			// sends map to client
			foreach (LevelGeometry gm in geometry) {
				SendToClient(client, String.Format("{0} {1} {2} {3} {4} {5} {6} {7}", NetMsg.DL_LEVEL_GEOMETRY, gm.position.X, gm.position.Y, gm.size.X, gm.size.Y, gm.color.R, gm.color.G, gm.color.B));
			}
		}

		void HandleDisconnect(Client client) {
			clients.Remove(client);
			SendToAll(NetMsg.PEER_LEFT +" "+ client.character.id);
		}

		void HandleChat() { }

		void HandlePing(Client client) {
			SendToClient(client, NetMsg.PONG + "");
			//Console.WriteLine("pinged");
			client.keepalive = 0;
		}

		void HandleNetworkMessage(Received received) {
			string[] words = received.Message.Split(' ');

			NetMsg command;
			Enum.TryParse(words[0], true, out command);
			// has to be specially handled anyway..
			if (command == NetMsg.CONNECT) {
				HandleConnectAttempt(received, words);
				return;
			}
		
			// no other messages should be accepted from not-connected
			if (!IsClientConnected(received.Sender))
				return;

			var client = GetClient(received.Sender);

			switch (command) {
				case NetMsg.DISCONNECT:
					HandleDisconnect(client);
					break;
				case NetMsg.CHAT:
					HandleChat();
					break;
				case NetMsg.PING:
					HandlePing(client);
					break;
				case NetMsg.PONG:
					// TODO:
					break;
				case NetMsg.C_LEFT_DOWN:
					//Console.WriteLine("LEFT DOWN");
					client.character.moveLeft = true;
					break;
				case NetMsg.C_LEFT_UP:
					//Console.WriteLine("LEFT UP");
					client.character.moveLeft = false;
					break;
				case NetMsg.C_RIGHT_DOWN:
					//Console.WriteLine("RIGHT DOWN");
					client.character.moveRight = true;
					break;
				case NetMsg.C_RIGHT_UP:
					//Console.WriteLine("RIGHT UP");
					client.character.moveRight = false;
					break;
				case NetMsg.C_JUMP_DOWN:
					//Console.WriteLine("JUMP DOWN");
					client.character.moveJump = true;
					break;
				case NetMsg.C_JUMP_UP:
					//Console.WriteLine("JUMP UP");
					client.character.moveJump = false;
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
