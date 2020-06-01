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
using RunGun.Server.Plugins;

/* TODO: make server occasionally talk shit in chat
 * TODO: RCON system
 * TODO: Lua scriptable gamemodes
 * TODO: further develop the plugin API
 */
namespace RunGun.Server
{
	interface IPluginEventArgs {}

	public delegate object[] LuaFunc(params object[] obj);

	interface IPluginAPI
	{
		public event LuaFunc OnServerStart;
		public event LuaFunc OnServerStop;
		public event LuaFunc OnConnectRequested;
		PluginManager Plugins { get; set; }
	}
	class Server : BaseServer, IGameController, IPluginAPI
	{

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
			SendToAll(new SPDeleteEntity(EntityID));
		}
		private void NetReplicatePlayer(Player player) {
			SendToAll(new SPAddPlayer(
				player.EntityID,
				player.Color.R,
				player.Color.G,
				player.Color.B,
				player.UserNickname
			));
		}
		private void NetReplicateBullet(Bullet bullet) {
			SendToAll(new SPAddBullet(
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

		public override void BindTo(IPEndPoint endpoint) {
			base.BindTo(endpoint);
			StartListening();
		}

		public Server() : base() {
			// init fields
			Plugins = new PluginManager();

			Plugins.LoadPlugins();
			World = new GameWorld();
			LoadTestMap();
			#region Listener Bindings
			AddListener<CInputState>(Protocol.C_InputState, InputStateListener);
			AddListener<CPing>(Protocol.C_Ping, PingListener);
			AddListener<CChat>(Protocol.C_ChatMessage, ChatListener);
			#endregion
			

			TaskManager.Register(new IntervalTask(GameStateTickRate, NetworkGameStateUpdate));

		}
		~Server() { }

		private void DownloadMap(User user) {
			SMapHeader header = new SMapHeader { };
			List<SMapSlice> data = new List<SMapSlice>();
			foreach (LevelGeometry geom in World.levelGeometries) {
				SMapSlice slice = new SMapSlice() {
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
			// TODO: design and implement leaderboard system
			SLeaderboardLayoutHeader header = new SLeaderboardLayoutHeader {

			};
			List<SLeaderboardLayoutSlice> data = new List<SLeaderboardLayoutSlice>();

			//foreach (var something in leaderboard) {
				
			//}
		}
		private void DownloadExistingEntities(User user) {
			foreach (var ent in World.entities) {
				if (ent is Bullet bullet) {
					Send(user, new SPAddBullet(
						bullet.EntityID,
						bullet.CreatorID,
						bullet.Direction
					));
				}

				if (ent is Player player) {
					Send(user, new SPAddPlayer(
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
		protected override (bool accept, string reason) OnConnectingCheck(INetworkPeer peer, CConnectRequest packet) {
			return base.OnConnectingCheck(peer, packet);
		}
		protected override void OnUserConnect(User user) {
			base.OnUserConnect(user);
			Send(user, new SPChat(string.Format("[Server] connected to {0} ({1})", ServerName, ListeningEndpoint.ToString())));

			GlobalMessage(user.Nickname + " joined.", ConsoleColor.Gray);
			DownloadMap(user);
			DownloadLeaderboardLayout(user);
			
			SendToAllExcept(user, new SPPeerJoin(user.NetworkID));

			Player player = new Player() {
				UserNickname = user.Nickname,
				UserGUID = user.NetworkID,
				Color = GetRandomColor(),
			};
			
			DownloadExistingEntities(user);
			SpawnEntity(player);
			Send(user, new SPAssignPlayerID(player.EntityID));
		}

		protected override void OnUserDisconnect(User user) {
			base.OnUserDisconnect(user);
			var player = GetPlayerOfUser(user);

			if (player == null) {
				// something's gone wrong for sure...
				return;
			}

			SendToAllExcept(user, new SPPeerLeft(user.NetworkID));
			SendToAllExcept(user, new SPDeleteEntity(player.EntityID));

			GlobalMessage(user.Nickname + " left.", ConsoleColor.Gray);
			World.RemoveEntity(player);
		}

		void ChatListener(User user, CChat packet) {
			// ? will chat string need sanitization later
			// TODO: test if strings can break stuff by not being sanitized
			GlobalMessage(user.Nickname + " : "+packet.Message, ConsoleColor.White);
		}
		void PingListener(User user, CPing packet) {
			Send(user, new SPPingReply());
			user.KeepAlive = 0;
		}

		void InputStateListener(User user, CInputState packet) {
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
			Random rand = new Random();
			World.levelGeometries = new List<LevelGeometry>() {
				new LevelGeometry(new Vector2(0, 20), new Vector2(20, 300), new Color(255, 0, 0)),
				new LevelGeometry(new Vector2(10, 300), new Vector2(500, 10), new Color(0, 255, 128)),
				new LevelGeometry(new Vector2(200, 300), new Vector2(20, 100), new Color(128, 0, 128)),
				new LevelGeometry(new Vector2(rand.Next(25, 310), rand.Next(20, 200)), new Vector2(20, 100), new Color(255, 0, 128)),
				new LevelGeometry(new Vector2(rand.Next(20, 200), rand.Next(100, 300)), new Vector2(rand.Next(40, 80), rand.Next(20, 200)), new Color(64, 255, 64)),
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
			SendToAll(new SPChat(message));
		}


		SGameStateHeader header;


		List<SGameStateSlice> states = new List<SGameStateSlice>();
		private void NetworkGameStateUpdate() {
			var header = new SGameStateHeader(World.physicsFrameIter);

			states.Clear();
			//List<S_GameStateSlice> states = new List<S_GameStateSlice>();
			// TODO: big optimization candidate
			foreach (var ent in World.GetEntities()) {
				if (ent is IPhysical phys) {
					var newState = new SGameStateSlice {
						EntityID = phys.EntityID,
						NextX = phys.NextPosition.X,
						NextY = phys.NextPosition.Y,
						VelocityX = phys.Velocity.X,
						VelocityY = phys.Velocity.Y
					};

					// TODO: come up with better flag handler than this.
					if (ent is Player plr) {
						newState.Flag0 = (byte)plr.Facing;
						newState.Flag1 = (byte)plr.Looking;
					}
					states.Add(newState);
				}
			}
			SendToAll(header, states.ToArray());
		}
	}
}
