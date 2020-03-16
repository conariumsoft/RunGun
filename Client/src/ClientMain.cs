using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client.Misc;
using RunGun.Client.Networking;
using RunGun.Core;
using RunGun.Core.Networking;
using RunGun.Core.Physics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RunGun.Client
{

    struct InputState
    {
        public bool w;
        public bool a;
        public bool d;
    }

    public class ClientMain : Game
    {
        public int ourPID;

        public static SpriteFont font;

        ClientArchitecture client;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<CLevelGeometry> map = new List<CLevelGeometry>();
        float pingClock = 0;
        float keepAlive = 0;
        float ping;
        Matrix screenTransform;
        
        Texture2D rectTexture;
        KeyListener listenW;
        KeyListener listenA;
        KeyListener listenD;
        bool connected = false;

        Vector2 textpos = new Vector2(4, 4);

        ChatSystem chat;
        GameWorld world;

        // TODO: play around and figure out nessecary buffer size (current = 1000)
        
        Disk<InputState> inputHistory = new Disk<InputState>(1000);

        FrameCounter frameCounter;

        Player GetLocalPlayer() {
            return (Player)world.GetEntity(ourPID);
        }

        void OnWDown() {
            client.Send(ClientCommand.MOVE_JUMP);
        }
        void OnWUp() {
            client.Send(ClientCommand.MOVE_STOP_JUMP);
        }
        void OnADown() {
            client.Send(ClientCommand.MOVE_LEFT);

        }
        void OnAUp() {
            client.Send(ClientCommand.MOVE_STOP_LEFT);
        }
        void OnDDown() {
            client.Send(ClientCommand.MOVE_RIGHT);
        }
        void OnDUp() {
            client.Send(ClientCommand.MOVE_STOP_RIGHT);
        }

        static string reconstruct(List<string> list) {
            string ret = "";

            foreach(var bit in list) {
                ret += bit;
                ret += " ";
            }
            ret.Trim();
            return ret;

        }

        void OnChatMessage(string message) {
            chat.ReceivedMessage(message);
        }

        void OnConnectionAccepted(Guid ourID, int sPhysFrame) {
            connected = true;
            world.physicsFrameIter = sPhysFrame;
        }

        void OnConnectionDenied(string reason) {

        }

        void OnGetLocalPlayerID(int pid) {
            ourPID = pid;
        }

        void OnReceiveMapData(Vector2 pos, Vector2 size, Color color) {

            map.Add(new CLevelGeometry(GraphicsDevice, pos, size, color));
        }

        void OnPingReply() {
            // before we reset keepAlive, that is our ping
            ping = keepAlive;
            keepAlive = 0;
        }

        void OnPing() {
            client.Send(ClientCommand.PING_REPLY);
        }
        void OnAddEntity(string entityType, int entityID) {

            if (entityType == "Player") {
                var plr = new Player(entityID);

                world.AddEntity(plr);
            }
        }

        void OnDeleteEntity(int entityID) {
            world.RemoveEntity(entityID);
        }

        void OnEntityPosition(int id, int step, Vector2 pos, Vector2 nextPos, Vector2 vel) {

            if (!world.HasEntity(id)) return; // should prolly bitch about this
            var entity = world.GetEntity(id);

            int diff = world.physicsFrameIter - step;

            entity.position = pos;

            if (entity is PhysicalEntity e) {
                e.nextPosition = nextPos;
                e.velocity = vel;

                if (entity is Player p ) {
                    if (p.EntityID == ourPID) {
                        var state = world.GetState(e, step);
                        if (state == null) return;

                        world.SetState(e, step, new EntityGameState {
                            position = p.position,
                            velocity = p.velocity,
                            nextPosition = p.nextPosition,
                            step = step,
                        });

                        for (int i = step; i < world.physicsFrameIter; i++) {
                            var input = inputHistory.Get(i);

                            p.moveLeft = input.a;
                            p.moveRight = input.d;
                            p.moveJump = input.w;

                            world.ProcessEntityPhysics(p, PhysicsProperties.PHYSICS_TIMESTEP, i);

                            world.SetState(p, i, new EntityGameState {
                                position = p.position,
                                velocity = p.velocity,
                                nextPosition = p.nextPosition,
                                step = step
                            });
                        }
                    }
                }
            }
        }

        // when player sends chat message
        void OnPlayerSendChat(string message) {
            client.Send(ClientCommand.SAY, message);
        }

        public ClientMain()
        {
            world = new GameWorld();

            world.OnPhysicsStep += PhysicsCallback;

            listenW = new KeyListener(Keys.W, OnWDown, OnWUp);
            listenA = new KeyListener(Keys.A, OnADown, OnAUp);
            listenD = new KeyListener(Keys.D, OnDDown, OnDUp);

            client = new ClientArchitecture();

            chat = new ChatSystem(OnPlayerSendChat);

            graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = 1024,
                PreferredBackBufferHeight = 640,
                SynchronizeWithVerticalRetrace = false,
                IsFullScreen = false,
            };
            Content.RootDirectory = "Content";

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

            client.Connect("127.0.0.1", 22222, "bastard");
        }

        protected override void Initialize() {
            base.Initialize();

            Window.TextInput += chat.OnTextInput;

            Window.AllowUserResizing = true;
            Window.AllowAltF4 = true;
            Window.Title = "RunGun Client";

            IsFixedTimeStep = false;

            frameCounter = new FrameCounter();

            Color[] data = new Color[32*32];

            rectTexture = new Texture2D(GraphicsDevice, 32, 32);

            for (int i = 0; i < data.Length; ++i) {
                data[i] = Color.White;
            }

            rectTexture.SetData(data);
            screenTransform = Matrix.CreateScale(1);
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = this.Content.Load<SpriteFont>("Font");
        }

        protected override void UnloadContent() {
            client.Send(ClientCommand.DISCONNECT);
        }

        // TODO: refactor this into core
        void PhysicsCallback(float step, int iterator) {

            var player = world.GetEntity(ourPID) as Player;

            positionHistory.Set(iterator, player.position);
            velocityHistory.Set(iterator, player.velocity);

            KeyboardState kbs = Keyboard.GetState();

            bool w = kbs.IsKeyDown(Keys.W);
            bool a = kbs.IsKeyDown(Keys.A);
            bool d = kbs.IsKeyDown(Keys.D);

            player.moveJump = w;
            player.moveLeft = a;
            player.moveRight = d;

            inputHistory.Set(iterator, new InputState() {
                w = w,
                a = a,
                d = d
            });
        }

        protected override void Update(GameTime gameTime)
        {
            frameCounter.Update(gameTime);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            client.Update(dt);
            chat.Update(dt);

            if (!connected)
                return;

            world.Update(dt);

            listenA.Update();
            listenW.Update();
            listenD.Update();

            pingClock += dt;
            keepAlive += dt;

            if (pingClock > 1.0f) {
                pingClock = 0;
                keepAlive = 0;

                client.Send(ClientCommand.PING);
            }

            if (keepAlive > 10.0f) {
                // TODO: lost connection to server
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, screenTransform);
            //-------------------------------------------------------------------------------------------------------------

            foreach (var geom in map) {
                geom.Draw(spriteBatch);
            }

            foreach (var entity in world.entities) {
                if (entity is Player p)
                    spriteBatch.Draw(rectTexture, p.GetDrawPosition(), Color.Blue);
            }

            double averageFPS = frameCounter.GetAverageFramerate();

            string debugdata = "fps: " + Math.Floor(averageFPS) + " ping: " + Math.Floor(ping * 1000) + "ms\n" +
                "entities: " + world.entities.Count + " ";

            spriteBatch.DrawString(font, debugdata, textpos, Color.White);

            chat.Draw(spriteBatch);

            //-------------------------------------------------------------------------------------------------------------
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
