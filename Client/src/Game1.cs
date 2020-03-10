using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client.Networking;
using RunGun.Core;
using RunGun.Core.Networking;
using RunGun.Core.Physics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RunGun.Client
{
    //public void OnKeyPress();
    public delegate void OnKeyRelease();

    public class KeyState
    {
        bool down;
        bool debounce;
        
        Keys key;

        Func<int> kp;
        Func<int> kr;


        public KeyState(Keys keyToListen, Func<int> onPress, Func<int> onRelease) {
            key = keyToListen;
            kp = onPress;
            kr = onRelease;

        }

        public void Update() {
            if (Keyboard.GetState().IsKeyDown(key)) {
                down = true;

                if (debounce == false) {
                    debounce = true;
                    kp();
                }

            } else {
                down = false;
                if (debounce == true) {
                    debounce = false;
                    kr();
                }
            }
        }
    }


    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        List<CLevelGeometry> map = new List<CLevelGeometry>();
        Stack<Received> networkMessageStack = new Stack<Received>();
        string username = "jommy";
        static UdpUser udpClient;
        float physicsClock;
        float pingClock = 0;
        float keepAlive = 0;
        float ping;
        Matrix screenTransform;
        static Player localPlayer;
        static Player replicatedPlayer;
        Texture2D rectTexture;
        KeyState listenW = new KeyState(Keys.W, OnWDown, OnWUp);
        KeyState listenA = new KeyState(Keys.A, OnADown, OnAUp);
        KeyState listenD = new KeyState(Keys.D, OnDDown, OnDUp);
        bool connected = false;

        public static int OnWDown() {
            SendToServer(NetMsg.C_JUMP_DOWN+"");
            return 0;
        }
        public static int OnWUp() {
            SendToServer(NetMsg.C_JUMP_UP + "");
            return 0;
        }
        public static int OnADown() {
            SendToServer(NetMsg.C_LEFT_DOWN + "");
            return 0;
        }
        public static int OnAUp() {
            SendToServer(NetMsg.C_LEFT_UP + "");
            return 0;
        }
        static int OnDDown() {
            SendToServer(NetMsg.C_RIGHT_DOWN + "");
            return 0;
        }
        static int OnDUp() {
            SendToServer(NetMsg.C_RIGHT_UP + "");
            return 0;
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = 1024,
                PreferredBackBufferHeight = 640,
                SynchronizeWithVerticalRetrace = false,
                IsFullScreen = false,
            };
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            base.Initialize();

            IsFixedTimeStep = false;

            localPlayer = new Player();
            replicatedPlayer = new Player();

            Color[] data = new Color[32*32];

            rectTexture = new Texture2D(GraphicsDevice, 32, 32);

            for (int i = 0; i < data.Length; ++i) {
                data[i] = Color.White;
            }

            rectTexture.SetData(data);
            screenTransform = Matrix.CreateScale(1);

            udpClient = UdpUser.Connect("127.0.0.1", 12345);
            SendToServer(String.Format("{0} {1}", NetMsg.CONNECT, username));

            Task.Factory.StartNew(async () => {
                while (true) {
                    try {
                        var received = await udpClient.Receive();

                        lock (networkMessageStack) {
                            networkMessageStack.Push(received);
                        }
                    } catch (Exception ex) {
                        Console.WriteLine(ex.StackTrace + "|" + ex.Message);
                    }
                }
            });
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent() {}


        void ProcessPhysics(Player e, float step) {

            e.Physics(step);

            e.isFalling = true;
            foreach (var geom in map) {
                bool result = CollisionSolver.CheckAABB(e.nextPosition, e.boundingBox, geom.GetCenter(), geom.GetDimensions());

                if (result) {
                    var separation = CollisionSolver.GetSeparationAABB(e.nextPosition, e.boundingBox, geom.GetCenter(), geom.GetDimensions());
                    var normal = CollisionSolver.GetNormalAABB(separation, e.velocity);

                    e.nextPosition += separation;
                    if (normal.Y == -1) {
                        e.isFalling = false;

                        if (!e.moveLeft && !e.moveRight) {
                            e.velocity = new Vector2(e.velocity.X * 0.9f, e.velocity.Y);
                        }
                    }
                }
            }
        }

        // TODO: refactor this into core
        void Physics(float step) {
            ProcessPhysics(localPlayer, step);
            ProcessPhysics(replicatedPlayer, step);
        }

        public static void SendToServer(string message) {
            udpClient.Send(message);
        }

        CLevelGeometry DecodeLevelGeometry(string[] message) {
            float x = float.Parse(message[1]);
            float y = float.Parse(message[2]);
            float w = float.Parse(message[3]);
            float h = float.Parse(message[4]);
            float r = float.Parse(message[5]);
            float g = float.Parse(message[6]);
            float b = float.Parse(message[7]);

            return new CLevelGeometry(GraphicsDevice, new Vector2(x, y), new Vector2(w, h), new Color(r, g, b));
        }


        void HandleNetworkMessage(Received received) {
            string[] words = received.Message.Split(' ');

            NetMsg command;
            Enum.TryParse(words[0], true, out command);

            switch (command) {
                case NetMsg.PONG:
                    // before we reset keepAlive, that is our ping
                    ping = keepAlive;
                    //Console.WriteLine("ping: " + ping);
                    keepAlive = 0;
                    break;
                case NetMsg.CONNECT_ACK:
                    connected = true;
                    break;
                case NetMsg.PING:
                    SendToServer(NetMsg.PONG + "");
                    break;
                case NetMsg.DL_LEVEL_GEOMETRY:

                    map.Add(DecodeLevelGeometry(words));
                    break;

                case NetMsg.YOUR_POS:
                    float x = float.Parse(words[1]);
                    float y = float.Parse(words[2]);
                    float velX = float.Parse(words[3]);
                    float velY = float.Parse(words[4]);

                    replicatedPlayer.nextPosition = new Vector2(x, y);
                    replicatedPlayer.velocity = new Vector2(velX, velY);

                    if (Vector2.Distance(localPlayer.nextPosition, replicatedPlayer.nextPosition) > 25) {
                        //Console.WriteLine("ZOG");

                        // epic LERP
                        localPlayer.nextPosition = Vector2.Lerp(localPlayer.nextPosition, replicatedPlayer.nextPosition, 0.5f);
                    }

                    break;

                default:
                    Console.WriteLine("Unexpected Command: [" + words[0] + "] full message: " + received.Message);
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

        protected override void Update(GameTime gameTime)
        {

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Window.Title = String.Format("RunGunClient {0}fps {1}ms", (1/dt), ping*1000);

            HandleNetworkStack();
            if (!connected)
                return;

            listenA.Update();
            listenW.Update();
            listenD.Update();

            localPlayer.Update(dt);
            replicatedPlayer.Update(dt);

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

                SendToServer(NetMsg.PING + "");
            }

            if (keepAlive > 10.0f) {
                // TODO: lost connection to server
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState kbs = Keyboard.GetState();

            localPlayer.moveJump = kbs.IsKeyDown(Keys.W);
            localPlayer.moveLeft = kbs.IsKeyDown(Keys.A);
            localPlayer.moveRight = kbs.IsKeyDown(Keys.D);


            base.Update(gameTime);
            //Console.WriteLine("fps: " + (1 / dt));
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, screenTransform);
            //-------------------------------------------------------------------------------------------------------------

            foreach (var geom in map) {
                geom.Draw(spriteBatch);
            }

            spriteBatch.Draw(rectTexture, localPlayer.GetDrawPosition(), Color.White);
            spriteBatch.Draw(rectTexture, replicatedPlayer.GetDrawPosition(), Color.Green);

            //-------------------------------------------------------------------------------------------------------------
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
