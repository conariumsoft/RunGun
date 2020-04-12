using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using RunGun.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Client.Input
{
	class TouchInput : IInput
	{
		public bool MovingLeft { get; set; }
		public bool MovingRight { get; set; }
		public bool Jumping { get; set; }
		public bool Shooting { get; set; }
		public bool LookingDown { get; set; }
		public bool LookingUp { get; set; }
		public bool InChat { get; set; }

		public void Update(float delta) {
			MovingLeft = false;
			MovingRight = false;
			Jumping = false;
			Shooting = false;
			LookingUp = false;
			LookingDown = false;

			foreach (TouchLocation touch in TouchPanel.GetState()) {

				if (touch.Position.Y < 80) {
					Jumping = true;
				}

				if (touch.Position.Y > 300) {
					Shooting = true;
				}

				if (touch.Position.X < 100) {
					MovingLeft = true;
				}

				if (touch.Position.X > 500) {
					MovingRight = true;
				}
			}
		}

		public void Draw(SpriteBatch spriteBatch, GraphicsDevice gdev) {

			int vpWidth = gdev.Viewport.Width;
			int vpHeight = gdev.Viewport.Height;

			ShapeRenderer.Rect(spriteBatch, Color.White, new Vector2(vpWidth-50, vpHeight-50), new Vector2(30, 30));
		}
	}
}
