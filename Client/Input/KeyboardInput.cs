using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Client.Input
{
	class KeyboardInput : IInput
	{

		#region Keybinds
		public Keys MovingLeftKey   { get; set; } = Keys.A;
		public Keys MovingRightKey  { get; set; } = Keys.D;
		public Keys JumpingKey      { get; set; } = Keys.Space;
		public Keys ShootingKey     { get; set; } = Keys.LeftShift;
		public Keys LookingUpKey    { get; set; } = Keys.W;
		public Keys LookingDownKey  { get; set; } = Keys.S;
		#endregion

		#region Action states
		public bool MovingLeft { get; set; }
		public bool MovingRight { get; set; }
		public bool Jumping { get; set; }
		public bool Shooting { get; set; }
		public bool LookingDown { get; set; }
		public bool LookingUp { get; set; }
		public bool InChat { get; set; }

		#endregion

		public void Update(float delta) {
			if (InChat)
				return;

			KeyboardState kbState = Keyboard.GetState();

			MovingLeft = kbState.IsKeyDown(MovingLeftKey);
			MovingRight = kbState.IsKeyDown(MovingRightKey);
			Jumping = kbState.IsKeyDown(JumpingKey);
			Shooting = kbState.IsKeyDown(ShootingKey);
			LookingDown = kbState.IsKeyDown(LookingDownKey);
			LookingUp = kbState.IsKeyDown(LookingUpKey);
		}

		public void Draw(SpriteBatch sb, GraphicsDevice gd) {
			throw new NotImplementedException();
		}
	}
}
