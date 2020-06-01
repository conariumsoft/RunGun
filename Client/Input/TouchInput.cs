using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using RunGun.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Client.Input
{
	class TouchPosition
	{
		public TouchPosition(int x, int y, int width, int height) {
			Rect = new Rectangle(x, y, width, height);
		}

		private Rectangle Rect;
		public bool IsTouched { get; private set; }

		public void Update() {
			IsTouched = false;
			foreach (TouchLocation touch in TouchPanel.GetState()) {
				// for some reason touch input position is doubled
				// investigate
				if (Rect.Contains(touch.Position/2)) {
					IsTouched = true;
				}
			}
		}

		public void Draw(SpriteBatch spriteBatch) {
			Color color = IsTouched ? Color.White : Color.DarkGray;
			ShapeRenderer.Rect(spriteBatch, color, new Vector2(Rect.X, Rect.Y), new Vector2(Rect.Width, Rect.Height));
		}
	}

	class TouchInput : IInput
	{
		public bool MovingLeft { get; set; }
		public bool MovingRight { get; set; }
		public bool Jumping { get; set; }
		public bool Shooting { get; set; }
		public bool LookingDown { get; set; }
		public bool LookingUp { get; set; }
		public bool InChat { get; set; }

		TouchPosition TPLeft;
		TouchPosition TPRight;
		TouchPosition TPJump;
		TouchPosition TPShoot;
		TouchPosition TPShootUp;
		TouchPosition TPShootDown;

		public TouchInput() {
			int w = GameConstants.BaseWidth;
			int h = GameConstants.BaseHeight;
			TPLeft = new TouchPosition(5, h-45, 50, 40);
			TPRight = new TouchPosition(65, h-45, 50, 40);
			TPJump = new TouchPosition(30, h-80, 60, 30);
			TPShoot = new TouchPosition(w - 45, h - 70, 40, 30); 
			TPShootUp = new TouchPosition(w - 45, h - 105, 40, 30);
			TPShootDown = new TouchPosition(w-45, h-35, 40, 30);
		}

		public void Update(float delta) {
			TPLeft.Update();
			TPRight.Update();
			TPJump.Update();
			TPShoot.Update();
			TPShootUp.Update();
			TPShootDown.Update();

			MovingLeft = TPLeft.IsTouched;
			MovingRight = TPRight.IsTouched;
			Jumping = TPJump.IsTouched;
			Shooting = (TPShoot.IsTouched || TPShootUp.IsTouched || TPShootDown.IsTouched);
			LookingUp = TPShootUp.IsTouched;
			LookingDown = TPShootDown.IsTouched;
		}

		public void Draw(SpriteBatch spriteBatch, GraphicsDevice gdev) {
			TPLeft.Draw(spriteBatch);
			TPRight.Draw(spriteBatch);
			TPJump.Draw(spriteBatch);
			TPShoot.Draw(spriteBatch);
			TPShootUp.Draw(spriteBatch);
			TPShootDown.Draw(spriteBatch);

			foreach (TouchLocation touch in TouchPanel.GetState()) {
				ShapeRenderer.Rect(spriteBatch, Color.Green, touch.Position, new Vector2(4, 4));
			}
		}
	}
}
