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
        public static SpriteFont font;

        ClientArchitecture client;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<CLevelGeometry> map = new List<CLevelGeometry>();
        string username = "jommy";
        float physicsClock;
        float pingClock = 0;
        float keepAlive = 0;
        float ping;
        Matrix screenTransform;
       // static Player localPlayer;
        static Player replicatedPlayer;
        Texture2D rectTexture;
        KeyListener listenW;
        KeyListener listenA;
        KeyListener listenD;
        bool connected = false;

        Vector2 textpos = new Vector2(4, 4);
        List<Player> otherPlayers;

        ChatSystem chat;

        int iterator = 0;
        // TODO: play around and figure out nessecary buffer size (current = 1000)
        Disk<Vector2> positionHistory = new Disk<Vector2>(1000);
        Disk<Vector2> velocityHistory = new Disk<Vector2>(1000);
        Disk<InputState> inputHistory = new Disk<InputState>(1000);

        FrameCounter frameCounter;

        void OnWDown() {
            client.Send(ClientCommand.PLR_JUMP);
        }
        void OnWUp() {
            client.Send(ClientCommand.PLR_STOP_JUMP);
        }
        void OnADown() {
            client.Send(ClientCommand.PLR_LEFT);

        }
        void OnAUp() {
            client.Send(ClientCommand.PLR_STOP_LEFT);
        }
        void OnDDown() {
            client.Send(ClientCommand.PLR_RIGHT);
        }
        void OnDUp() {
            client.Send(ClientCommand.PLR_STOP_RIGHT);
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

        void OnConnectionDenied(string reason) {

        }

        void OnConnectionAccepted(int clientAssignedID, int sPhysFrame) {
            connected = true;

            iterator = sPhysFrame;
            //Console.WriteLine("iter: " + iterator);

            replicatedPlayer.id = clientAssignedID;
            // localPlayer.id = ourId;
        }

        void OnReceiveMapData(Vector2 pos, Vector2 size, Color color) {

            map.Add(new CLevelGeometry(GraphicsDevice, pos, size, color));
        }

        void OnPong() {
            // before we reset keepAlive, that is our ping
            ping = keepAlive;
            keepAlive = 0;
        }

        void OnPing() {
            client.Send(ClientCommand.PONG);
        }

        void OnPeerJoined(int peerID) {
            var newPlayer = new Player(peerID);
            Console.WriteLine("Uh oh");
            otherPlayers.Add(newPlayer);
        }

        void OnPeerLeft(int peerID) {
            foreach (var plr in otherPlayers.ToArray()) {
                if (plr.id == peerID) {
                    otherPlayers.Remove(plr);
                }
            }
        }
        void OnExistingPeer(int peerID) {
            var newPlr = new Player(peerID);
            Console.WriteLine("CUMMMM");
            otherPlayers.Add(newPlr);
        }

        void OnPlayerPosition(int id, Vector2 pos, Vector2 vel, int stepIter) {

            if (id == replicatedPlayer.id) {
                replicatedPlayer.nextPosition = pos;
                replicatedPlayer.velocity = vel;

                int diff = iterator - stepIter;
                int poo = iterator - diff;

                positionHistory.Set(poo, replicatedPlayer.nextPosition);
                velocityHistory.Set(poo, replicatedPlayer.velocity);

                for (int i = poo; i < iterator; i++) {

                    var input = inputHistory.Get(i);

                    replicatedPlayer.moveLeft = input.a;
                    replicatedPlayer.moveRight = input.d;
                    replicatedPlayer.moveJump = input.w;

                    ProcessPhysics(replicatedPlayer, PhysicsProperties.PHYSICS_TIMESTEP);
                    positionHistory.Set(i, replicatedPlayer.nextPosition);
                    velocityHistory.Set(i, replicatedPlayer.velocity);
                }
                iterator = stepIter;
            } else {
                foreach (var plr in otherPlayers) {
                    if (id == plr.id) {
                        plr.nextPosition = pos;
                        plr.velocity = vel;
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
           // localPlayer = new Player();
            replicatedPlayer = new Player(-1);
            otherPlayers = new List<Player>();

            client.OnConnectAccept += OnConnectionAccepted;
            client.OnConnectDenied += OnConnectionDenied;
            client.OnPeerJoined += OnPeerJoined;
            client.OnPeerLeft += OnPeerLeft;
            client.OnPlayerPosition += OnPlayerPosition;
            client.OnPong += OnPong;
            client.OnReceiveMapData += OnReceiveMapData;
            client.OnChatMessage += OnChatMessage;
            client.OnPing += OnPing;
            client.OnExistingPeer += OnExistingPeer;


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

        void ProcessPhysics(Player e, float step) {

            e.Physics(step);

            e.isFalling = true;
            foreach (var geom in map) {
                CollisionSolver.SolveEntityAgainstGeometry(e, geom);
            }
        }

        // TODO: refactor this into core
        void Physics(float step) {
            iterator++;

            positionHistory.Set(iterator, replicatedPlayer.position);
            velocityHistory.Set(iterator, replicatedPlayer.velocity);

            inputHistory.Set(iterator, new InputState() {
                w = Keyboard.GetState().IsKeyDown(Keys.W),
                a = Keyboard.GetState().IsKeyDown(Keys.A),
                d = Keyboard.GetState().IsKeyDown(Keys.D)
            });

            ProcessPhysics(replicatedPlayer, step);

            foreach (var plr in otherPlayers) {
                ProcessPhysics(plr, step);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            frameCounter.Update(gameTime);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            client.Update(dt);
            chat.Update(dt);

            if (!connected)
                return;

            listenA.Update();
            listenW.Update();
            listenD.Update();

            //localPlayer.Update(dt);
            replicatedPlayer.Update(dt);

            foreach (var plr in otherPlayers) {
                plr.Update(dt);
            }

            physicsClock += dt;
            while (physicsClock > PhysicsProperties.PHYSICS_TIMESTEP) {
                physicsClock -= PhysicsProperties.PHYSICS_TIMESTEP;
                Physics(PhysicsProperties.PHYSICS_TIMESTEP);
            }

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

            KeyboardState kbs = Keyboard.GetState();

            replicatedPlayer.moveJump = kbs.IsKeyDown(Keys.W);
            replicatedPlayer.moveLeft = kbs.IsKeyDown(Keys.A);
            replicatedPlayer.moveRight = kbs.IsKeyDown(Keys.D);


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

            foreach (var plr in otherPlayers) {
                spriteBatch.Draw(rectTexture, plr.GetDrawPosition(), Color.Blue);
            }

            //spriteBatch.Draw(rectTexture, localPlayer.GetDrawPosition(), Color.White);
            spriteBatch.Draw(rectTexture, replicatedPlayer.GetDrawPosition(), Color.Green);

            double averageFPS = frameCounter.GetAverageFramerate();

            string debugdata = "fps: " + Math.Floor(averageFPS) + " ping: " + Math.Floor(ping*1000) + "ms\n"+
                "peers: " + otherPlayers.Count + " ";

            spriteBatch.DrawString(font, debugdata, textpos, Color.White);


            chat.Draw(spriteBatch);

            //-------------------------------------------------------------------------------------------------------------
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
