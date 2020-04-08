using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client.Networking;
using RunGun.Core;
using RunGun.Core.Game;
using RunGun.Core.Generic;
using RunGun.Core.Networking;
using RunGun.Core.Physics;
using RunGun.Core.Rendering;
using RunGun.Core.Utility;
using System;
using System.Collections.Generic;
using System.Net;

namespace RunGun.Client
{
	#region BigBoy
	struct InputState {
		public bool jump;
		public bool left;
		public bool right;
		public bool shoot;
	}

	public enum GameState
	{
		MainMenu,
		InGame,
		DownloadingData,
		Connecting,
		Disconnected,
		Other
	}
	#endregion

	public abstract class BaseClient : Game, IGameController
	{

		public bool GraphicsDebugEnabled { get; set; }
		public int ViewportWidth { get; set; } = 1024;
		public int ViewportHeight { get; set; } = 640;
		public bool VsyncEnabled { get; set; } = false;

		public string Nickname { get; set; } = "baseplayer";

		protected GameState CurrentGameState { get; set; }

		#region Graphics
		protected GraphicsDeviceManager graphicsDeviceManager;
		SpriteBatch spriteBatch;
		protected Matrix screenTransform;
		#endregion

		NetworkClient Client;

		protected InputManager Input { get; set; }
		public IChatSystem Chat { get; set; } = new BaseChatSystem();
		public GameWorld World { get; set; }
		CircularArray<InputState> inputHistory = new CircularArray<InputState>(1000);
		public short ourPID; // our player ID
		float pingClock = 0;
		float keepAlive = 0;
		float ping;
		bool receivedPID = false;

		FrameCounter frameCounter;


		#region Misc variables
		Vector2 textpos = new Vector2(4, 4);

		#endregion

		public BaseClient() {
			// fixed shit
			Content.RootDirectory = "Content";
			IsFixedTimeStep = false;
			// etc
			graphicsDeviceManager = new GraphicsDeviceManager(this) {
				PreferredBackBufferHeight = ViewportHeight,
				PreferredBackBufferWidth = ViewportWidth,
				SynchronizeWithVerticalRetrace = VsyncEnabled,
				PreferMultiSampling = false,
			};
			CurrentGameState = GameState.MainMenu;
			screenTransform = Matrix.CreateScale(1);
			World = new GameWorld();
			Client = new NetworkClient();
			#region Listener Bindings
			Client.AddListener<S_ConnectAcceptPacket>(Protocol.S_ConnectOK, OnServerAccept);
			Client.AddListener<S_ConnectDenyPacket>(Protocol.S_ConnectDeny, OnServerDeny);
			Client.AddListener<S_ChatPacket>(Protocol.S_Chat, OnServerChatMsg);
			Client.AddListener<S_AssignPlayerIDPacket>(Protocol.S_AssignPlayerID, OnAssignedPlayerID);
			Client.AddListener<S_PingReplyPacket>(Protocol.S_PingReply, OnServerPingReply);
			Client.AddListener<S_AddBulletPacket>(Protocol.S_AddBullet, OnAddBullet);
			Client.AddListener<S_AddPlayerPacket>(Protocol.S_AddPlayer, OnAddPlayer);
			Client.AddListener<S_DeleteEntityPacket>(Protocol.S_DeleteEntity, OnDeleteEntity);
			Client.AddListener
				<S_LeaderboardLayoutHeader, S_LeaderboardLayoutSlice>
				(Protocol.S_LeaderboardLayout, OnLeaderboardLayout);
			Client.AddListener
				<S_GameStateHeader, S_GameStateSlice>
				(Protocol.S_GameState, OnGameStateUpdate);
			Client.AddListener
				<S_MapHeader, S_MapSlice>
				(Protocol.S_MapData, OnReceiveMapData);
			#endregion
			frameCounter = new FrameCounter();
			World.OnPhysicsStep += PhysicsCallback;

			TaskManager.Register(new IntervalTask(1, PingServer));
			TaskManager.Register(new IntervalTask(30, SendInputState));
		}

		#region Listener Methods (Network Bindings)
		private void OnServerAccept(S_ConnectAcceptPacket packet) {

		}
		private void OnServerDeny(S_ConnectDenyPacket packet) {

		}
		private void OnServerChatMsg(S_ChatPacket packet) {
			Chat.AddMessage(new ChatMessage() { 
				Text = packet.Message,
				TextColor = Color.White,
			});
		}
		private void OnAssignedPlayerID(S_AssignPlayerIDPacket packet) {
			receivedPID = true;
			ourPID = packet.PlayerID;
		}
		private void OnServerPing() {
			Client.Send(new C_PingReplyPacket());
			pingClock = 0;
		}
		private void OnServerPingReply(S_PingReplyPacket packet) {
			ping = pingClock;
			Console.WriteLine("Ping: "+ ping);
			keepAlive = 0;
	
		}
		private void OnGameStateUpdate(S_GameStateHeader header, List<S_GameStateSlice> slices) {
			
			int physicsFrame = World.physicsFrameIter;
			World.physicsFrameIter = header.PhysicsStep;

			foreach (S_GameStateSlice slice in slices) {
				if (!World.HasEntity(slice.EntityID)) 
					continue;

				IPhysical ent = World.GetEntity(slice.EntityID) as IPhysical;
				// if state implements Position, make sure to add here
				ent.NextPosition = new Vector2(slice.NextX, slice.NextY);
				ent.Velocity = new Vector2(slice.VelocityX, slice.VelocityY);

				for (int i = header.PhysicsStep; i < physicsFrame; i++) {
					if (ent is Player player && player.EntityID == ourPID) {
						var input = inputHistory.Get(i);
						player.MovingLeft = input.left;
						player.MovingRight = input.right;
						player.Jumping = input.jump;
					}
					World.ProcessEntityPhysics(ent, PhysicsProperties.PHYSICS_TIMESTEP);
				}
			}
		}
		private void OnLeaderboardLayout(S_LeaderboardLayoutHeader header, List<S_LeaderboardLayoutSlice> slices) {

		}
		private void OnReceiveMapData(S_MapHeader header, List<S_MapSlice> slices) {
			foreach (S_MapSlice slice in slices) {
				World.levelGeometries.Add(new LevelGeometry(
					new Vector2(slice.X, slice.Y),
					new Vector2(slice.Width, slice.Height),
					new Color(slice.R, slice.G, slice.B)
				));
			}
		}
		private void OnAddPlayer(S_AddPlayerPacket packet) {
			Player player = new Player() {
				EntityID = packet.EntityID,
				UserNickname = packet.Nickname,
				Color = new Color(packet.Red, packet.Green, packet.Blue)
			};
			World.AddEntity(player);
		}
		private void OnAddBullet(S_AddBulletPacket packet) {
			Bullet bullet = new Bullet() {
				Direction = packet.Direction,
				CreatorID = packet.CreatorID,
				EntityID = packet.EntityID,
			};
			World.AddEntity(bullet);
		}
		private void OnDeleteEntity(S_DeleteEntityPacket packet) {
			World.RemoveEntity(packet.EntityID);
		}
		#endregion

		private void PingServer() {
			Client.Send(new C_PingPacket());
		}

		private void SendInputState() {
			bool has = World.HasEntity(ourPID);
			
			if (has == false) {
				return;
			}
			Player player = World.GetEntity(ourPID) as Player;
			var packet = new C_InputStatePacket(player.MovingLeft, player.MovingRight, player.Jumping, player.Shooting, player.LookingUp, player.LookingDown);

			Client.Send(packet);
		}

		public virtual void ConnectToServer(IPEndPoint endpoint) {
			Client.Connect(endpoint, Nickname);
		}

		public void OnLocalChatMessage(string message) {

			Client.Send(new C_ChatPacket(message));
		}

		#region Monogame Overrides (Init, LoadContent, Update, Draw)
		protected override void Initialize() {
			base.Initialize();
			
		}
		protected override void LoadContent() {
			base.LoadContent();

			spriteBatch = new SpriteBatch(GraphicsDevice);
			
			ShapeRenderer.Initialize(GraphicsDevice);
			TextRenderer.Initialize(Content);
		}
		protected override void UnloadContent() {
			Client.Disconnect();
		}
		protected override void Update(GameTime gameTime) {
			frameCounter.Update(gameTime);
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

			TaskManager.Update(dt);
			Input.Update(dt);
			Client.Update(dt);

			if (Client.IsConnected)
				UpdateGame(dt);
			else if (Client.IsConnecting)
				UpdateConnecting(dt);
			else
				UpdateUnconnected(dt);

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {

				Client.Disconnect();
				Exit();
			}

			base.Update(gameTime);
		}
		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(Color.Black);
			spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, screenTransform);
			//-------------------------------------------------------------------------------------------------------------

			if (Client.IsConnected)
				DrawGame();
			else if (Client.IsConnecting)
				DrawConnecting();
			else
				DrawUnconnected();

			//-------------------------------------------------------------------------------------------------------------
			spriteBatch.End();
			base.Draw(gameTime);
		}
		#endregion

		void PhysicsCallback(float step, int iterator) {
			if (receivedPID == false) {
				return;
			}
			var player = World.GetEntity(ourPID) as Player;
			//KeyboardState kbs = Keyboard.GetState();

			bool jump = Input.IsUserJumping();
			bool left = Input.IsUserMovingLeft();
			bool right = Input.IsUserMovingRight();
			bool shoot = Input.IsUserShooting();
			bool lookup = Input.IsUserLookingUp();
			bool lookdown = Input.IsUserLookingDown();

			player.Shooting = shoot;
			player.Jumping = jump;
			player.MovingLeft = left;
			player.MovingRight = right;
			player.LookingUp = lookup;
			player.LookingDown = lookdown;

			inputHistory.Set(iterator, new InputState() {
				jump = jump,
				left = left,
				right = right,
				shoot = shoot,
			});
		}

		#region Game Logic Methods

		#region In-Game State (connected to server)
		protected virtual void UpdateGame(float dt) {
			World.ClientUpdate(this, dt);
			Chat.Update(dt);
			World.Update(dt);

			keepAlive += dt;
			if (keepAlive > 10.0f) {
				Console.WriteLine("Lost Conn...");
				Client.Disconnect();
			}
		}
		protected virtual void DrawGame() {
			foreach (LevelGeometry geom in World.levelGeometries)
				geom.Draw(spriteBatch);

			// entity list
			ShapeRenderer.Rect(spriteBatch, new Color(0.5f, 0.5f, 0.5f), new Vector2(), new Vector2());

			foreach (var entity in World.entities) {
				if (entity is IDrawableRG drawableEntity) {
					drawableEntity.Draw(spriteBatch);
				}
			}

			double averageFPS = frameCounter.GetAverageFramerate();

			string debugdata = String.Format(
				"fps: {0} ping: {1}ms ent: {2} download {3}kb upload {4}kb, avg {5}kb/s {6}kb/s", 
				Math.Floor(averageFPS), Math.Floor(ping * 1000), World.entities.Count, 
				Math.Floor(Client.DataTotalIn/1000.0), Math.Floor(Client.DataTotalOut/1000.0),
				Math.Floor(Client.DataAverageIn/100)/50, Math.Floor(Client.DataAverageOut/100)/50);

			TextRenderer.Print(spriteBatch, debugdata, textpos, Color.White);

			Chat.Draw(spriteBatch);
		}
		#endregion

		#region Connecting State
		protected virtual void UpdateConnecting(float dt) { }
		protected virtual void DrawConnecting() {
			TextRenderer.Print(spriteBatch, "Attempting connection to server.", textpos, Color.White);
		}
		#endregion

		#region Unconnected State
		protected virtual void UpdateUnconnected(float dt) { }
		protected virtual void DrawUnconnected() {
			TextRenderer.Print(spriteBatch, "No connection established to server.", textpos, Color.White);
		}
		#endregion

		#endregion

		public void SpawnEntity(IEntity entity) {
			throw new NotImplementedException();
		}

		public void RemoveEntity(short id) {
			throw new NotImplementedException();
		}
	}
}
