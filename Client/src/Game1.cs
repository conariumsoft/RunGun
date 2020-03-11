using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client.Misc;
using RunGun.Client.Networking;
using RunGun.Core;
using RunGun.Core.Networking;
using RunGun.Core.Physics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RunGun.Client
{

    struct InputState
    {
        public bool w;
        public bool a;
        public bool d;
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
       // static Player localPlayer;
        static Player replicatedPlayer;
        Texture2D rectTexture;
        KeyListener listenW = new KeyListener(Keys.W, OnWDown, OnWUp);
        KeyListener listenA = new KeyListener(Keys.A, OnADown, OnAUp);
        KeyListener listenD = new KeyListener(Keys.D, OnDDown, OnDUp);
        bool connected = false;

        SpriteFont font;
        Vector2 textpos = new Vector2(4, 4);
        List<Player> otherPlayers;


        int iterator = 0;
        Vector2[] positionHistory = new Vector2[100000];
        Vector2[] velocityHistory = new Vector2[100000];
        InputState[] inputHistory = new InputState[100000];

        FrameCounter frameCounter;

        static int OnWDown() {
            SendToServer(NetMsg.C_JUMP_DOWN+"");
            return 0;
        }
        static int OnWUp() {
            SendToServer(NetMsg.C_JUMP_UP + "");
            return 0;
        }
        static int OnADown() {
            SendToServer(NetMsg.C_LEFT_DOWN + "");
            return 0;
        }
        static int OnAUp() {
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
           // localPlayer = new Player();
            replicatedPlayer = new Player();
            otherPlayers = new List<Player>();
        }

        protected override void Initialize() {
            base.Initialize();

            IsFixedTimeStep = false;

            frameCounter = new FrameCounter();

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
                        //Thread.Sleep(); // false lag
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


            font = this.Content.Load<SpriteFont>("Font");
        }

        protected override void UnloadContent() {
            SendToServer(NetMsg.DISCONNECT + "");
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

            positionHistory[iterator] = replicatedPlayer.nextPosition;
            velocityHistory[iterator] = replicatedPlayer.velocity;
            inputHistory[iterator] = new InputState() {
                w = Keyboard.GetState().IsKeyDown(Keys.W),
                a = Keyboard.GetState().IsKeyDown(Keys.A),
                d = Keyboard.GetState().IsKeyDown(Keys.D)
            };

            //ProcessPhysics(localPlayer, step);
            ProcessPhysics(replicatedPlayer, step);


            foreach(var plr in otherPlayers) {
                ProcessPhysics(plr, step);
            }
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

                    int ourId = int.Parse(words[1]);
                    int serverIter = int.Parse(words[2]);

                    iterator = serverIter;
                    Console.WriteLine("iter: " + iterator);

                    replicatedPlayer.id = ourId;
                   // localPlayer.id = ourId;

                    connected = true;
                    break;
                case NetMsg.PING:
                    SendToServer(NetMsg.PONG + "");
                    break;
                case NetMsg.DL_LEVEL_GEOMETRY:

                    map.Add(DecodeLevelGeometry(words));
                    break;

                case NetMsg.PLAYER_POS:
                    int id = int.Parse(words[1]);
                    float x = float.Parse(words[2]);
                    float y = float.Parse(words[3]);
                    float velX = float.Parse(words[4]);
                    float velY = float.Parse(words[5]);
                    int stepIter = int.Parse(words[6]);

                    if (id == replicatedPlayer.id) {
                        replicatedPlayer.nextPosition = new Vector2(x, y);
                        replicatedPlayer.velocity = new Vector2(velX, velY);

                        int diff = iterator - stepIter;
                        Console.WriteLine("cock: " + iterator + " " + stepIter + " " + diff);

                        int poo = iterator - diff;

                        positionHistory[poo] = replicatedPlayer.nextPosition;
                        velocityHistory[poo] = replicatedPlayer.velocity;

                        for (int i = poo; i < iterator; i++) {

                            replicatedPlayer.moveLeft = inputHistory[i].a;
                            replicatedPlayer.moveRight = inputHistory[i].d;
                            replicatedPlayer.moveJump = inputHistory[i].w;

                            ProcessPhysics(replicatedPlayer, PhysicsProperties.PHYSICS_TIMESTEP);
                            positionHistory[i] = replicatedPlayer.nextPosition;
                            velocityHistory[i] = replicatedPlayer.velocity;
                        }

                    } else {
                        foreach (var plr in otherPlayers) {
                            if (id == plr.id) {
                                plr.nextPosition = new Vector2(x, y);
                                plr.velocity = new Vector2(velX, velY);
                            }
                        }
                    }
                    break;

                case NetMsg.PEER_JOINED:
                    int idOf = int.Parse(words[1]);

                    var newPlayer = new Player();
                    newPlayer.id = idOf;

                    otherPlayers.Add(newPlayer);
                    break;
                case NetMsg.PEER_LEFT:
                    // TODO: get rid of player?

                    int idT = int.Parse(words[1]);

                    foreach (var plr in otherPlayers.ToArray()) {
                        if (plr.id == idT) {
                            otherPlayers.Remove(plr);
                        }
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
            frameCounter.Update(gameTime);

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            HandleNetworkStack();
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

                SendToServer(NetMsg.PING + "");
            }

            if (keepAlive > 10.0f) {
                // TODO: lost connection to server
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            KeyboardState kbs = Keyboard.GetState();

            //localPlayer.moveJump = kbs.IsKeyDown(Keys.W);
            //localPlayer.moveLeft = kbs.IsKeyDown(Keys.A);
            //localPlayer.moveRight = kbs.IsKeyDown(Keys.D);

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

            //-------------------------------------------------------------------------------------------------------------
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
