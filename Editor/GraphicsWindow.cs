using System;
using System.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameControls;
using RunGun.Core.Rendering;

namespace Editor
{
    public class GraphicsWindow : MonoGameViewModel
    {
        public IViewModelEditor Editor { get; set; }
        public GraphicsWindow() {
            Editor = MapEditor.Instance;
        }

        private SpriteBatch SpriteBatch;

        public override void LoadContent()
        {
            
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            ShapeRenderer.Initialize(GraphicsDevice);
            TextRenderer.Initialize(Content);

            Editor.Initialize();
            
            //Editor.OnViewportChange(GraphicsDevice);
        }

        public override void SizeChanged(object sender, SizeChangedEventArgs args) {
            base.SizeChanged(sender, args);

          //  if (GraphicsDevice!=null) {
                Editor.OnViewportChange((int)args.NewSize.Width, (int)args.NewSize.Height);
         //   }
            //
        }
        public override void Update(GameTime gameTime) {
            // MapEditor.Camera.ViewportWidth = GraphicsDevice.Viewport.Width;
            //MapEditor.Camera.ViewportHeight = GraphicsDevice.Viewport.Height;
            Editor.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            Editor.Draw(GraphicsDevice, SpriteBatch);
        }
    }
}