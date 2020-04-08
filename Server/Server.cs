using Microsoft.Xna.Framework;
using RunGun.Core;
using RunGun.Core.Networking;
using RunGun.Server.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using RunGun.Core.Utility;
using RunGun.Core.Game;
using System.Text;

/* TODO: make server occasionally talk shit in chat
 * TODO: RCON system
 * TODO: Lua scriptable gamemodes
 * 
 * 
 */
namespace RunGun.Server
{

	interface IPluginEventArgs {}

	public delegate object[] LuaFunc(params object[] obj);

	interface IPluggableServer
	{
		public event LuaFunc OnServerStart;
		public event LuaFunc OnServerStop;
		public event LuaFunc OnConnectRequested;
		PluginManager Plugins { get; set; }
	}
	class Server : BaseServer, IGameController, IPluggableServer {

		#region Server Configuration Properties
		public int MinimumThreadSleepTime { get; set; } = 8; // minimum time (ms) between each gametick.
		public float GameStateTickRate { get; set; } = 30.0f; //  times per second to replicate the gamestate to clients
		public int MaxPlayers { get; set; } = 64;

		#endregion
		private bool isServerRunning = true;
		private float deltaTime = 0;
		private short entityIDIterator = 0;

		#region Server Plugin Events
		public event LuaFunc OnServerStart;
		public event LuaFunc OnServerStop;
		public event LuaFunc OnConnectRequested;

		public PluginManager Plugins { get; set; }
		#endregion

		public GameWorld World { get; set; }

		Leaderboard leaderboard;

		#region Various Helper Methods
		private Player GetPlayerOfUser(User user) {
			foreach (var ent in World.GetEntities())
				if (ent is Player p)
					if (p.UserNickname == user.Nickname)
						return p;
			return null;
		}
		
		public void SpawnEntity(IEntity entity) {
			entity.EntityID = entityIDIterator;
			entityIDIterator++;
			if (entity is Bullet bullet) {
				NetReplicateBullet(bullet);
			}
			if (entity is Player player) {
				NetReplicatePlayer(player);
			}
			World.AddEntity(entity);
		}

		public void RemoveEntity(short EntityID) {
			SendToAll(new S_DeleteEntityPacket(EntityID));
		}
		private void NetReplicatePlayer(Player player) {
			SendToAll(new S_AddPlayerPacket(
				player.EntityID,
				player.Color.R,
				player.Color.G,
				player.Color.B,
				player.UserNickname
			));
		}
		private void NetReplicateBullet(Bullet bullet) {
			SendToAll(new S_AddBulletPacket(
				bullet.EntityID,
				bullet.CreatorID,
				bullet.Direction
			));
		}

		private Color GetRandomColor() {
			Random r = new Random();
			return new Color(r.Next(0, 256), r.Next(0, 256), r.Next(0, 256));
		}
		#endregion

		public Server(IPEndPoint endpoint) : base(endpoint) {
			// init fields
			Plugins = new PluginManager();

			Plugins.LoadPlugins();
			World = new GameWorld();
			LoadTestMap();
			#region Listener Bindings
			AddListener<C_InputStatePacket>(Protocol.C_InputState, InputStateListener);
			AddListener<C_PingPacket>(Protocol.C_Ping, PingListener);
			AddListener<C_ChatPacket>(Protocol.C_ChatMessage, ChatListener);
			#endregion
			StartListening();

			TaskManager.Register(new IntervalTask(GameStateTickRate, NetworkGameStateUpdate));

		}
		~Server() { }

		private void DownloadMap(User user) {
			S_MapHeader header = new S_MapHeader { };
			List<S_MapSlice> data = new List<S_MapSlice>();
			foreach (LevelGeometry geom in World.levelGeometries) {
				S_MapSlice slice = new S_MapSlice() {
					X =      (short)geom.Position.X,
					Y =      (short)geom.Position.Y,
					Width =  (short)geom.Size.X,
					Height = (short)geom.Size.Y,
					R = geom.Color.R,
					G = geom.Color.G,
					B = geom.Color.B
				};
				data.Add(slice);
			}
			Send(user, header, data.ToArray());
		}
		private void DownloadLeaderboardLayout(User user) {
			S_LeaderboardLayoutHeader header = new S_LeaderboardLayoutHeader {

			};
			List<S_LeaderboardLayoutSlice> data = new List<S_LeaderboardLayoutSlice>();

			//foreach (var something in leaderboard) {
				// TODO: figure out
			//}
		}
		private void DownloadExistingEntities(User user) {
			foreach (var ent in World.entities) {
				if (ent is Bullet bullet) {
					Send(user, new S_AddBulletPacket(
						bullet.EntityID,
						bullet.CreatorID,
						bullet.Direction
					));
				}

				if (ent is Player player) {
					Send(user, new S_AddPlayerPacket(
						player.EntityID,
						player.Color.R,
						player.Color.G,
						player.Color.B,
						player.UserNickname
					));
				}
			}
		}

		#region Listener Methods (Network Bindings)
		protected override (bool accept, string reason) OnConnectingCheck(INetworkPeer peer, C_ConnectRequestPacket packet) {
			return base.OnConnectingCheck(peer, packet);
		}
		protected override void OnUserConnect(User user) {
			base.OnUserConnect(user);
			// TODO: add ip and port
			Send(user, new S_ChatPacket(string.Format("[Server] connected to {0} ({1})", ServerName, ListeningEndpoint.ToString())));

			GlobalMessage(user.Nickname + " joined.", ConsoleColor.Gray);
			DownloadMap(user);
			DownloadLeaderboardLayout(user);
			
			SendToAllExcept(user, new S_PeerJoinPacket(user.NetworkID));
			Player player = new Player();
			player.UserNickname = user.Nickname;
			player.UserGUID = user.NetworkID;
			player.Color = GetRandomColor();
			DownloadExistingEntities(user);
			SpawnEntity(player);
			Send(user, new S_AssignPlayerIDPacket(player.EntityID));
		}

		protected override void OnUserDisconnect(User user) {
			base.OnUserDisconnect(user);
			var player = GetPlayerOfUser(user);

			if (player == null) {
				// something's gone wrong for sure...
				return;
			}

			SendToAllExcept(user, new S_PeerLeftPacket(user.NetworkID));
			SendToAllExcept(user, new S_DeleteEntityPacket(player.EntityID));
			// TODO:
			GlobalMessage(user.Nickname + " left.", ConsoleColor.Gray);
			World.RemoveEntity(player);
		}

		void ChatListener(User user, C_ChatPacket packet) {
			// TODO: Sanitize string?
			GlobalMessage(user.Nickname + " : "+packet.Message, ConsoleColor.White);
		}
		void PingListener(User user, C_PingPacket packet) {
			Send(user, new S_PingReplyPacket());
			user.KeepAlive = 0;
		}
		void InputStateListener(User user, C_InputStatePacket packet) {
			Player player = GetPlayerOfUser(user);

			//if (player == null) return;
			// i'd rather it crash RN, since this means something is BAD wrong (user exists, but player doesn't
			player.MovingLeft = packet.Left;
			player.MovingRight = packet.Right;
			player.Jumping = packet.Jump;
			player.Shooting = packet.Shoot;
			player.LookingDown = packet.LookDown;
			player.LookingUp = packet.LookUp;
		}
		#endregion

		#region Initialization Methods
		private void LoadTestMap() {
			World.levelGeometries = new List<LevelGeometry>() {
				new LevelGeometry(new Vector2(0, 20), new Vector2(20, 600), new Color(255, 0, 0)),
				new LevelGeometry(new Vector2(10, 500), new Vector2(1000, 40), new Color(0, 255, 128)),
				new LevelGeometry(new Vector2(400, 400), new Vector2(20, 50), new Color(128, 0, 128)),
			};
		}
		#endregion

		public override void Update(float delta) {
			base.Update(delta);
			TaskManager.Update(delta);
			//UpdateNetRelay(delta);
			World.Update(delta);
			World.ServerUpdate(this, delta);
		}
		public int Run() {
			StartListening();
			Stopwatch stopwatch = new Stopwatch();
			while (isServerRunning) {
				stopwatch = Stopwatch.StartNew();
				Update(deltaTime);
				Thread.Sleep(MinimumThreadSleepTime);
				deltaTime = (float)stopwatch.Elapsed.TotalSeconds;
			}
			StopListening();
			stopwatch.Stop();
			return 0;
		}

		public void GlobalMessage(string message, ConsoleColor color) {
			Logging.Out(message, color);
			SendToAll(new S_ChatPacket(message));
		}


		private void NetworkGameStateUpdate() {
			var header = new S_GameStateHeader(World.physicsFrameIter);

			List<S_GameStateSlice> states = new List<S_GameStateSlice>();
			// TODO: big optimization candidate
			foreach (var ent in World.GetEntities()) {
				if (ent is IPhysical phys) {
					states.Add(new S_GameStateSlice {
						EntityID = phys.EntityID,
						NextX = phys.NextPosition.X, NextY = phys.NextPosition.Y,
						VelocityX = phys.Velocity.X, VelocityY = phys.Velocity.Y
					});
				}
			}

			SendToAll(header, states.ToArray());
		}
	}
}
