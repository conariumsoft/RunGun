using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client.Networking;
using RunGun.Core.Networking;
using RunGun.Core.Physics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RunGun.Client
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Stack<Received> networkMessageStack;

        UdpUser udpClient;

        double networkClock;
        float physicsClock;
         
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            udpClient = UdpUser.Connect("127.0.0.1", 12345);
        }

        protected override void Initialize()
        {
            base.Initialize();

            Task.Factory.StartNew(async () => {
                while(true) {
                    try {
                        var received = await udpClient.Receive();

                        lock(networkMessageStack) {
                            networkMessageStack.Push(received);
                        }

                    } catch (Exception ex) {
                        // poo
                    }
                }
            });
        }

        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent() {}

        void Physics(float step) {

        }

        void HandleNetworkMessage(Received received) {
            string[] words = received.Message.Split(' ');

            switch (words[0]) {
                case "pong":

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

            HandleNetworkStack();

            double dt = gameTime.ElapsedGameTime.TotalSeconds;

            physicsClock += (float)dt;
            while (physicsClock < PhysicsProperties.PHYSICS_TIMESTEP) {
                physicsClock -= PhysicsProperties.PHYSICS_TIMESTEP;
                Physics(PhysicsProperties.PHYSICS_TIMESTEP);
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            base.Draw(gameTime);
        }
    }
}
