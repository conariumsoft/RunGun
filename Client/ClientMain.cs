using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using RunGun.Client.Networking;
using RunGun.Core;
using RunGun.Core.Game;
using RunGun.Core.Networking;
using RunGun.Core.Physics;
using RunGun.Core.Rendering;
using System;
using System.Text;

namespace RunGun.Client
{
	struct InputState {
		public bool jump;
		public bool left;
		public bool right;
		public bool shoot;
	}

	public static class GlobalDevices
	{
		public static GraphicsDeviceManager GraphicsDeviceManager;
		public static SpriteBatch SpriteBatch;
		public static GraphicsDevice GraphicsDevice;
		public static BasicEffect BasicEffect;
		public static ContentManager ContentManager;

#nullable enable
		public static GraphicsDeviceManager? GetGraphicsDeviceManager() {
			return GraphicsDeviceManager;
		}

		public static SpriteBatch? GetSpriteBatch() {
			return SpriteBatch;
		}

		public static GraphicsDevice? GetGraphicsDevice() {
			return GraphicsDevice;
		}

		public static ContentManager? GetContentManager() {
			return ContentManager;
		}
#nullable restore
	}

	public class ClientMain : Game
	{
		CNetworkLayer client;
		InputManager inputManager;
		public BaseChatSystem chat;
		GameWorld world;
		Disk<InputState> inputHistory = new Disk<InputState>(1000);
		FrameCounter frameCounter;
		public short ourPID; // our player ID
		float pingClock = 0;
		float keepAlive = 0;
		float ping;
		bool connected = false; // if client is connected
		Matrix screenTransform;
		Vector2 textpos = new Vector2(4, 4);

		public ClientMain(string nickname, string ipaddress, string port)
		{
			GlobalDevices.GraphicsDeviceManager = new GraphicsDeviceManager(this) {
				PreferredBackBufferWidth = 1024,
				PreferredBackBufferHeight = 640,
				SynchronizeWithVerticalRetrace = false,
				IsFullScreen = false,
				PreferMultiSampling = false,
			};
			
			Content.RootDirectory = "Content";

			world = new GameWorld();
			client = new CNetworkLayer();
			frameCounter = new FrameCounter();

			DetermineInputMode();
			ConnectInputCallbacks();
			ConnectPacketCallbacks();

			world.OnPhysicsStep += PhysicsCallback;

			screenTransform = Matrix.CreateScale(1);
			client.Connect(ipaddress, int.Parse(port), nickname); // TODO: wrap in try-catch, and make it retry..
		}

		#region Initialization_Methods
		private void DetermineInputMode() {
			bool isController = GamePadState.Default.IsConnected;
			bool isTouch = TouchPanel.GetCapabilities().IsConnected;

			if (isController) {
				inputManager = new InputManager(InputMode.CONTROLLER);
				Console.WriteLine("Started game in Controller mode");
			} else if (isTouch) {
				inputManager = new InputManager(InputMode.TOUCH);
				Console.WriteLine("Started game in Touch mode");
			} else {
				inputManager = new InputManager(InputMode.KEYBOARD);
				Console.WriteLine("Started game in Keyboard mode");
			}
		}

		private void ConnectInputCallbacks() {
			inputManager.OnStartJump += OnPlayerStartJump;
			inputManager.OnStopJump += OnPlayerStopJump;
			inputManager.OnStartMoveLeft += OnPlayerStartMoveLeft;
			inputManager.OnStopMoveLeft += OnPlayerStopMoveLeft;
			inputManager.OnStartMoveRight += OnPlayerStartMoveRight;
			inputManager.OnStopMoveRight += OnPlayerStopMoveRight;
			inputManager.OnStartShoot += OnPlayerStartShoot;
			inputManager.OnStopShoot += OnPlayerStopShoot;
			inputManager.OnStartLookUp += OnPlayerStartLookUp;
			inputManager.OnStopLookUp += OnPlayerStopLookUp;
			inputManager.OnStartLookDown += OnPlayerStartLookDown;
			inputManager.OnStopLookDown += OnPlayerStopLookDown;

		}
		private void ConnectPacketCallbacks() {
			client.OnAddEntity += OnAddEntity;
			client.OnDeleteEntity += OnDeleteEntity;
			client.OnGetLocalPlayerID += OnGetLocalPlayerID;
			client.OnConnectAccept += OnConnectionAccepted;
			client.OnConnectDenied += OnConnectionDenied;
			client.OnEntityPosition += OnEntityPosition;
			client.OnPingReply += OnPingReply;
			client.OnReceiveMapData += OnReceiveMapData;
			client.OnChatMessage += OnChatMessage;
			client.OnPing += OnPing;
		}

		#endregion

		protected override void Initialize() {
			base.Initialize();
			Window.AllowUserResizing = true;
			Window.AllowAltF4 = true;
			Window.Title = "RunGun Client";
			IsFixedTimeStep = false;
		}

		protected override void LoadContent() {
			// ALWAYS define these before initializing ANYTHING
			GlobalDevices.ContentManager = Content;
			GlobalDevices.SpriteBatch = new SpriteBatch(GraphicsDevice);
			GlobalDevices.GraphicsDevice = GraphicsDevice;

			ShapeRenderer.Initialize(GlobalDevices.GetGraphicsDevice(), GlobalDevices.GetSpriteBatch());
			TextRenderer.Initialize(GlobalDevices.GetSpriteBatch(), GlobalDevices.GetContentManager());
		}

		protected override void UnloadContent() {
			client.Send(P_Disconnect());
		}


		#region Packet_Creation_Methods
		private static byte[] P_Disconnect() {
			return new byte[] { (byte)ClientCommand.DISCONNECT };
		}
		private static byte[] P_Say(string message) {
			//! if using non-ascii characters, need to change to UTF-8. (i.e regions with non-latin alphabets)
			var msg = Encoding.ASCII.GetBytes(message);
			var b = new byte[msg.Length + 1];
			b[0] = (byte)ClientCommand.CHAT_SAY;

			Array.Copy(msg, 0, b, 1, msg.Length);
			return b;
		}
		#endregion


		#region Received_Packet_Callbacks
		void OnChatMessage(string message) {
			chat.ReceivedMessage(message);
		}

		void OnConnectionAccepted(Guid ourID, int sPhysFrame) {
			connected = true;
			world.physicsFrameIter = sPhysFrame;
		}

		void OnConnectionDenied(string reason) {

		}

		void OnGetLocalPlayerID(short pid) {
			ourPID = pid;
		}

		void OnReceiveMapData(Vector2 pos, Vector2 size, Color color) {
			world.levelGeometries.Add(new LevelGeometry(pos, size, color));
		}

		void OnPingReply() {
			// before we reset keepAlive, that is our ping
			ping = keepAlive;
			keepAlive = 0;
		}

		void OnPing() {
			//client.Send(ClientCommand.PING_REPLY);
		}
		void OnAddEntity(Entity e) {
			world.AddEntity(e);
		}

		void OnDeleteEntity(short entityID) {
			world.RemoveEntity(entityID);
		}

		void OnEntityPosition(short id, int step, Vector2 pos, Vector2 nextPos, Vector2 vel) {

			if (!world.HasEntity(id)) return; // should prolly bitch about this
			var entity = world.GetEntity(id);

			entity.Position = pos;

			entity.NextPosition = nextPos;
			entity.Velocity = vel;

			if (entity is Player p && p.EntityID == ourPID) {
				int physicsFrame = world.physicsFrameIter;
				world.physicsFrameIter = step;
				for (int i = step; i < physicsFrame; i++) {
					var input = inputHistory.Get(i);

					p.moveLeft = input.left;
					p.moveRight = input.right;
					p.moveJump = input.jump;

					world.ProcessEntityPhysics(p, PhysicsProperties.PHYSICS_TIMESTEP);
				}
			}
		}

		// when player sends chat message
		public void OnPlayerSendChat(string message) {
			client.Send(P_Say(message));
		}
		#endregion
		// these are all input callbacks, used to tell server about input changes.
		byte[] C(ClientCommand command) {
			return new byte[] { (byte)command };
		}

		#region Input_Senders
		void OnPlayerStartJump() { client.Send(C(ClientCommand.MOVE_JUMP)); }
		void OnPlayerStopJump() { client.Send(C(ClientCommand.MOVE_STOP_JUMP)); }
		void OnPlayerStartMoveLeft() { client.Send(C(ClientCommand.MOVE_LEFT)); }
		void OnPlayerStopMoveLeft() { client.Send(C(ClientCommand.MOVE_STOP_LEFT)); }
		void OnPlayerStartMoveRight() { client.Send(C(ClientCommand.MOVE_RIGHT)); }
		void OnPlayerStopMoveRight() { client.Send(C(ClientCommand.MOVE_STOP_RIGHT)); }
		void OnPlayerStartLookDown() { client.Send(C(ClientCommand.LOOK_DOWN)); }
		void OnPlayerStopLookDown() { client.Send(C(ClientCommand.STOP_LOOK_DOWN)); }
		void OnPlayerStartLookUp() { client.Send(C(ClientCommand.LOOK_UP)); }
		void OnPlayerStopLookUp() { client.Send(C(ClientCommand.STOP_LOOK_UP)); }
		void OnPlayerStartShoot() { client.Send(C(ClientCommand.SHOOT)); }
		void OnPlayerStopShoot() { client.Send(C(ClientCommand.STOP_SHOOT)); }
		#endregion


		// TODO: refactor this into core
		void PhysicsCallback(float step, int iterator) {

			var player = world.GetEntity(ourPID) as Player;

			KeyboardState kbs = Keyboard.GetState();

			bool jump = inputManager.IsUserJumping();
			bool left = inputManager.IsUserMovingLeft();
			bool right = inputManager.IsUserMovingRight();

			player.moveJump = jump;
			player.moveLeft = left;
			player.moveRight = right;

			inputHistory.Set(iterator, new InputState() {
				jump = jump,
				left = left,
				right = right
			});
		}

		protected override void Update(GameTime gameTime) {
			frameCounter.Update(gameTime);
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

			inputManager.Update(dt);

			client.Update(dt);
			chat.Update(dt);

			if (!connected)
				return;

			world.Update(dt);
			world.ClientUpdate(dt);

			pingClock += dt;
			keepAlive += dt;

			if (pingClock > 1.0f) {
				pingClock = 0;
				keepAlive = 0;

				//client.Send(ClientCommand.PING);
			}

			if (keepAlive > 10.0f) {
				// TODO: lost connection to server
			}

			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) {
				client.Send(P_Disconnect());
				Exit();
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			SpriteBatch spriteBatch = GlobalDevices.GetSpriteBatch();
			spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, screenTransform);
			//-------------------------------------------------------------------------------------------------------------


			foreach (LevelGeometry geom in world.levelGeometries)
				geom.Draw();
			

			foreach (var entity in world.entities)
				entity.Draw();
			

			double averageFPS = frameCounter.GetAverageFramerate();

			string debugdata = String.Format("fps: {0} ping: {1}ms ent: {2}", Math.Floor(averageFPS), Math.Floor(ping * 1000), world.entities.Count);

			TextRenderer.Print(Color.White, debugdata, textpos);

			chat.Draw();

			//-------------------------------------------------------------------------------------------------------------
			spriteBatch.End();
			base.Draw(gameTime);
		}
	}
}
