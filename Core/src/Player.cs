using Microsoft.Xna.Framework;
using RunGun.Core.Physics;
using System;

namespace RunGun.Core
{

	public class Player : PhysicalEntity
	{
		float walkAccelleration = 600;
		float maxWalkspeed = 150;
		float jumpPower = 1000;
		public bool moveLeft;
		public bool moveRight;
		public bool moveJump;
		public float health = 100;
		public float maxHealth = 100;
		public float defense = 0;
		public bool destroyWhenDead = false;

		public int id;

		public Player() {
			position = new Vector2(64, 64);
			nextPosition = new Vector2(64, 64);
			boundingBox = new Vector2(16, 16);

			id = -1;
		}

		public Vector2 GetDrawPosition() {
			return position - (boundingBox);
		}

		public override void Update(double delta) {}

		public override void OnCollide(Vector2 separation, Vector2 normal) {
			base.OnCollide(separation, normal);
			if (normal.Y == -1) {
				isFalling = false;

				if (!moveLeft && !moveRight) {
					velocity = new Vector2(velocity.X * 0.9f, velocity.Y);
				}
			}
		}

		public override void Physics(float step) {
			base.Physics(step);

			float x_thrust = 0;
			float y_thrust = 0;

			if (moveLeft && velocity.X > -maxWalkspeed) {
				x_thrust = (-walkAccelleration * step);
			}
			if (moveRight && velocity.X < maxWalkspeed) {
				x_thrust += (walkAccelleration * step);
			}
			if (moveJump && isFalling == false) {
				isFalling = true;
				y_thrust = -(jumpPower * mass);
			}

			velocity += new Vector2(x_thrust, y_thrust);
		}
	}
}