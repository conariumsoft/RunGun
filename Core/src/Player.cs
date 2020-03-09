using Microsoft.Xna.Framework;
using RunGun.Core.Physics;

namespace RunGun.Core
{
	public class Player : PhysicalEntity, ILiving
	{
		float WalkAccelleration { get; set; }
		float MaxWalkspeed { get; set; }
		float JumpPower { get; set; }
		
		public bool MoveLeft { get; set; }
		public bool MoveRight { get; set; }
		public bool MoveJump { get; set; }

		public float Health { get; set; }
		public float MaxHealth { get; set; }
		public float Defense { get; set; }
		public bool DestroyWhenDead { get; set; }

		public Player() {
			Position = new Vector2(64, 64);
			NextPosition = new Vector2(64, 64);
			BoundingBox = new Vector2(16, 16);
		}

		public override void Update(double delta) {
			
		}

		public override void Physics(float step) {
			base.Physics(step);

			float x_thrust = 0;
			float y_thrust = 0;

			if (MoveLeft && Velocity.X > -MaxWalkspeed) {
				x_thrust = (-WalkAccelleration * step);
			}
			if (MoveRight && Velocity.X < MaxWalkspeed) {
				x_thrust += (WalkAccelleration * step);
			}
			if (MoveJump && IsFalling == false) {
				y_thrust = -(JumpPower * Mass);
			}

			Velocity += new Vector2(x_thrust, y_thrust);
		}
	}
}