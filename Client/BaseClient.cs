using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client.Input;
using RunGun.Client.Networking;
using RunGun.Client.Rendering;
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
	struct InputState {
		public bool Jump;
		public bool Left;
		public bool Right;
		public bool Shoot;
		public bool Up;
		public bool Down;
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
	
	public abstract class BaseClient : Game, IGameController
	{
		public const float PreferredAspectRatio = 16 / (float)9;
		public const int   BaseWidth = 640;
		public const int   BaseHeight = 360;

		#region Game subsystem classes
		protected NetworkClient Client { get; }
		protected FrameCounter  FrameCounter { get; }
		protected Camera        Camera { get; }
		protected IInput        Input { get; set; }
		public    IChat         Chat { get; set; }
		public    GameWorld     World { get; set; }
		#endregion

		#region Monogame class stuff
		protected GraphicsDeviceManager GraphicsDeviceManager;
		protected SpriteBatch SpriteBatch;
		#endregion

		#region Configuration values
		public bool   GraphicsDebugEnabled { get; set; }
		public int    ViewportWidth        { get; set; } = 1280;
		public int    ViewportHeight       { get; set; } = 720;
		public bool   VsyncEnabled         { get; set; } = false;
		public string Nickname             { get; set; } = "baseplayer";
		#endregion

		protected GameState CurrentGameState { get; set; }

		CircularArray<InputState> InputHistory { get; }
		
		float pingClock = 0; // network latency clock
		float keepAlive = 0; // is server responding?
		float ping;

		public short ourPID; // our player ID
		bool receivedPID = false; // have we received our assigned PID from server?

		private void CalculateViewportScale() {
			float clientAspect = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;

			float scale;
			
			// output is taller than wider, bars on top/bottom
			if (clientAspect <= PreferredAspectRatio) {
				
				int presentHeight = (int)((Window.ClientBounds.Width / PreferredAspectRatio) + 0.5f);
				int barHeight = (Window.ClientBounds.Height - presentHeight) / 2;

				Camera.ViewportOffset = new Vector2(0, barHeight);
				scale = Window.ClientBounds.Height /(float) BaseHeight;
			// output is wider than it is tall, put bars left/right
			} else {
				int presentWidth = (int)((Window.ClientBounds.Height * PreferredAspectRatio) + 0.5f);
				int barWidth = (Window.ClientBounds.Width - presentWidth) / 2;

				Camera.ViewportOffset = new Vector2(barWidth, 0);
				scale = Window.ClientBounds.Width /(float) BaseWidth;
			}
			Camera.Zoom = scale;
			Camera.ViewportHeight = GraphicsDevice.Viewport.Height;
			Camera.ViewportWidth = GraphicsDevice.Viewport.Width;
		}

		public void OnResize(object sender, EventArgs e) {
			CalculateViewportScale();
		}

		public BaseClient() {
			FrameCounter = new FrameCounter();
			Camera = new Camera();
			World = new GameWorld();
			Client = new NetworkClient();
			InputHistory = new CircularArray<InputState>(1000);

			Content.RootDirectory = "Content";
			IsFixedTimeStep = false;
			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += new EventHandler<EventArgs>(OnResize);

			GraphicsDeviceManager = new GraphicsDeviceManager(this) {
				PreferredBackBufferHeight = ViewportHeight,
				PreferredBackBufferWidth = ViewportWidth,
				SynchronizeWithVerticalRetrace = VsyncEnabled,
				PreferMultiSampling = false,
			};

			CurrentGameState = GameState.MainMenu;

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

			World.OnPhysicsStep += PhysicsCallback;

			TaskManager.Register(new IntervalTask(1.0f, PingServer));
			TaskManager.Register(new IntervalTask(20.0f, SendInputState));
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
		private void OnServerPing(S_PingPacket packet) {
			Client.Send(new C_PingReplyPacket());
			pingClock = 0;
		}
		private void OnServerPingReply(S_PingReplyPacket packet) {
			ping = pingClock;
			Console.WriteLine("Ping: " + ping);
			keepAlive = 0;

		}
		private void OnGameStateUpdate(S_GameStateHeader header, List<S_GameStateSlice> slices) {

			int physicsFrame = World.physicsFrameIter;
			World.physicsFrameIter = header.PhysicsStep;

			// grab each entity in the world
			// and make a dataslice for their physical state
			// send the batch off to clients to process
			foreach (S_GameStateSlice slice in slices) {
				if (World.HasEntity(slice.EntityID)) {
					IPhysical ent = World.GetEntity(slice.EntityID) as IPhysical;

					// if state implements Position, make sure to add here
					ent.NextPosition = new Vector2(slice.NextX, slice.NextY);
					ent.Velocity = new Vector2(slice.VelocityX, slice.VelocityY);

					for (int i = header.PhysicsStep; i < physicsFrame; i++) {
						if (ent is Player player && player.EntityID == ourPID) {

							var input = InputHistory.Get(i);
							player.MovingLeft = input.Left;
							player.MovingRight = input.Right;
							player.Jumping = input.Jump;
							player.LookingDown = input.Down;
							player.LookingUp = input.Up;
							//player.Shooting = input.Shoot;
						}
						World.ProcessEntityPhysics(ent, PhysicsProperties.PHYSICS_TIMESTEP);
					}
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
			if (Client.IsConnected)
				Client.Send(new C_PingPacket());
		}

		private void SendInputState() {
			if (Client.IsConnected == false) {
				return;
			}

			if (World.HasEntity(ourPID) == false) {
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

			SpriteBatch = new SpriteBatch(GraphicsDevice);

			CalculateViewportScale();

			ShapeRenderer.Initialize(GraphicsDevice);
			TextRenderer.Initialize(Content);
		}
		protected override void UnloadContent() {
			Client.Disconnect();
		}
		protected override void Update(GameTime gameTime) {
			FrameCounter.Update(gameTime);
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

		protected virtual void DrawGameLayer() {
			if (Client.IsConnected) {
				foreach (LevelGeometry geom in World.levelGeometries)
					geom.Draw(SpriteBatch);

				// entity list
				ShapeRenderer.Rect(SpriteBatch, new Color(0.5f, 0.5f, 0.5f), new Vector2(), new Vector2());

				foreach (var entity in World.entities) {
					if (entity is IRenderComponent drawableEntity) {
						drawableEntity.Draw(SpriteBatch);
					}
				}
			}
		}
		protected virtual void DrawUILayer() {
			
			if (Client.IsConnected) {
				Chat.Draw(SpriteBatch, GraphicsDevice);
			} else if (Client.IsConnecting) {
				TextRenderer.Print(SpriteBatch, "Attempting connection to server.", new Vector2(0, 0), Color.White);
			} else {
				TextRenderer.Print(SpriteBatch, "No connection established to server.", new Vector2(0, 0), Color.White);
			}
		}

		protected virtual void DrawDebugData() {

			string networkData = "";
			string graphicsData = "";


			graphicsData = String.Format("fps: {0} ", 
				Math.Floor(FrameCounter.GetAverageFramerate())
			);
			if (Client.IsConnected) {
				networkData = String.Format(
					"ping: {0}ms ent: {1} download {2}KiB upload {3}KiB, avg {4}KiB/s {5}KiB/s",
					Math.Floor(ping * 1000), 
					World.entities.Count,
					Math.Floor(Client.DataTotalIn / 1000.0),
					Math.Floor(Client.DataTotalOut / 1000.0),
					Math.Floor(Client.DataAverageIn / 100) / 50, 
					Math.Floor(Client.DataAverageOut / 100) / 50
				);
			}

			SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.ScreenSpaceMatrix);
			TextRenderer.Print(SpriteBatch, networkData, new Vector2(0, 0), Color.White);
			TextRenderer.Print(SpriteBatch, graphicsData, new Vector2(0, 16), Color.White);
			SpriteBatch.End();
		}

		private void DrawOverflowBox() {
			SpriteBatch.Begin();
			ShapeRenderer.Rect(SpriteBatch, Color.Black, new Vector2(0, 0), new Vector2(Camera.ViewportOffset.X, Window.ClientBounds.Height));
			ShapeRenderer.Rect(SpriteBatch, Color.Black, new Vector2(Window.ClientBounds.Width - Camera.ViewportOffset.X, 0), new Vector2(Camera.ViewportOffset.X, Window.ClientBounds.Height));
			ShapeRenderer.Rect(SpriteBatch, Color.Black, new Vector2(0, 0), new Vector2(Window.ClientBounds.Width, Camera.ViewportOffset.Y));
			ShapeRenderer.Rect(SpriteBatch, Color.Black, new Vector2(0, Window.ClientBounds.Height - Camera.ViewportOffset.Y), new Vector2(Window.ClientBounds.Width, Camera.ViewportOffset.Y));
			SpriteBatch.End();
		}

		protected override void Draw(GameTime gameTime) {
			GraphicsDevice.Clear(ClearOptions.Target, Color.SteelBlue, 1.0f, 0);
			
			SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.WorldSpaceMatrix);
			//ShapeRenderer.Rect(SpriteBatch, Color.Blue, Camera.Position- new Vector2(BaseWidth, BaseHeight)/2, new Vector2(BaseWidth, BaseHeight));
			DrawGameLayer();
			SpriteBatch.End();

			SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Camera.ScreenSpaceMatrix);
			DrawUILayer();
			SpriteBatch.End();

			
			DrawDebugData();
			

			DrawOverflowBox();
			base.Draw(gameTime);
		}
		#endregion

		void PhysicsCallback(float step, int iterator) {
			if (receivedPID == false) {
				return;
			}
			var player = World.GetEntity(ourPID) as Player;

			// center of screen should be the player
			Camera.Position = new Vector2((int)player.Position.X, (int)player.Position.Y);

			bool jump = Input.Jumping;
			bool left = Input.MovingLeft;
			bool right = Input.MovingRight;
			bool shoot = Input.Shooting;
			bool lookup = Input.LookingUp;
			bool lookdown = Input.LookingDown;

			player.Shooting = shoot;
			player.Jumping = jump;
			player.MovingLeft = left;
			player.MovingRight = right;
			player.LookingUp = lookup;
			player.LookingDown = lookdown;

			InputHistory.Set(iterator, new InputState() {
				Jump = jump,
				Left = left,
				Right = right,
				Shoot = shoot,
				Up = lookup,
				Down = lookdown,
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
	
		#endregion
		#region Connecting State
		protected virtual void UpdateConnecting(float dt) { }
		#endregion
		#region Unconnected State
		protected virtual void UpdateUnconnected(float dt) { }
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
