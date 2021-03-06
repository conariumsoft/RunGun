﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunGun.Core.Game.Guns;
using RunGun.Core.Physics;
using RunGun.Core.Rendering;
using System;
using System.Text;

namespace RunGun.Core.Game
{
	public enum Facing : byte { Left = 1, Right = 2 }
	public enum Looking : byte { Forward = 0, Up = 1, Down = 2}

	public enum MyEnum
	{
		ONE = 1,
		TWO = 2,
		THREE = 3,
		FOUR = 4
	}


	public class Player : PhysicalEntity, ILiving, ICollidable
	{

		private const float walkAccelleration = 1000;
		private const float maxWalkspeed = 200;
		private const float jumpPower = 450;
		private const float thrustPower = 100;

		public float bulletTimer = 0.5f;

		public bool MovingLeft { get; set; }
		public bool MovingRight { get; set; }
		public bool Jumping { get; set; }
		public bool Shooting { get; set; }
		public bool LookingUp { get; set; }
		public bool LookingDown { get; set; }
		public Vector2 BoundingBox { get; } = new Vector2(16, 16);
		public string UserNickname { get; set; }
		public Guid UserGUID { get; set; }
		public Color Color { get; set; }
		public int Health { get; set; } = 100;

		IFirearm EquippedGun { get; }

		public Facing Facing { get; set; } = Facing.Right;
		public Looking Looking { get; set; } = Looking.Forward;
		public Player() {
			NextPosition = new Vector2(30, 30);
		}
		public void Kick() {
			Console.WriteLine("GOT KICKED!");
		}

		public Vector2 GetDrawPosition() {
			return Position - (BoundingBox);
		}

		public override void Update(float delta) {
			base.Update(delta);

			//Console.WriteLine(Position);

			if (MovingLeft)  { Facing = Facing.Left;  }
			if (MovingRight) { Facing = Facing.Right; }

			if (LookingDown) { Looking = Looking.Down; }
			if (LookingUp)   { Looking = Looking.Up;   }
			
		}



		public void OnCollide(Vector2 separation, Vector2 normal) {
			//base.OnCollide(separation, normal);
			if (normal.Y == -1) {
				IsFalling = false;
				Velocity = new Vector2(Velocity.X, 0);

				if (!MovingLeft && !MovingRight) {
					Velocity = new Vector2(Velocity.X * 0.9f, Velocity.Y);
				}
			}
			if (normal.Y == 1) {
				Velocity = new Vector2(Velocity.X, -Velocity.Y * 0.3f);
			}
			if (normal.X != 0) {
				Velocity = new Vector2(0, Velocity.Y);
			}
		}
		public override void Physics(float step) {
			float x_thrust = 0;
			float y_thrust = 0;

			if (MovingLeft && Velocity.X > -maxWalkspeed) {
				x_thrust = (-walkAccelleration * step);
			}
			if (MovingRight && Velocity.X < maxWalkspeed) {
				x_thrust += (walkAccelleration * step);
			}
			if (Jumping && IsFalling == false) {
				IsFalling = true;
				if (MovingLeft) {
					x_thrust = -(thrustPower * Mass);
					y_thrust = -(jumpPower/2 * Mass);
				} else if (MovingRight) {
					x_thrust = (thrustPower * Mass);
					y_thrust = -(jumpPower/2 * Mass);
				} else {
					y_thrust = -(jumpPower * Mass);
				}
			}
			Velocity += new Vector2(x_thrust, y_thrust);
			base.Physics(step);
		}

		public override void Draw(SpriteBatch sb) {
			base.Draw(sb);
			ShapeRenderer.Rect(sb, Color, GetDrawPosition(), BoundingBox * 2);
			ShapeRenderer.Rect(sb, Color, (int)Position.X, (int)Position.Y, (int)BoundingBox.X, (int)BoundingBox.Y);
			Looking = Looking.Forward;
			float rotation = 0;
			
			if (LookingDown) {
				rotation = (float)Math.PI;
			}
				
			if (LookingUp) {
				
			}

			if (Facing == Facing.Right) {
				ShapeRenderer.Rect(sb, Color.DarkBlue, GetDrawPosition() + new Vector2(BoundingBox.X*2, 0), new Vector2(30, 10));
			}

			if (Facing == Facing.Left) {
				ShapeRenderer.Rect(sb, Color.DarkBlue, GetDrawPosition()-new Vector2(30, 0), new Vector2(30, 10));
			}

			//ShapeRenderer.Rect(sb, Color.DarkBlue, GetDrawPosition(), new Vector2(20, 8), rotation);

			TextRenderer.Print(sb, Health.ToString(), GetDrawPosition(), Color.Green);
		}

		private BulletDirection GetDirection() {
			if (LookingUp) {
				return BulletDirection.UP;
			} else if (LookingDown) {
				return BulletDirection.DOWN;
			} else if (Facing == Facing.Left) {
				return BulletDirection.LEFT;
			}
			return BulletDirection.RIGHT;
		}

		public override void ServerSideUpdate(IGameController gc, float dt) {
			base.ServerSideUpdate(gc, dt);

			bulletTimer += dt;

			if (Shooting && bulletTimer >= 0.05f) {
				bulletTimer = 0;

				gc.SpawnEntity(new Bullet() {
					CreatorID = this.EntityID,
					Position = Position,
					NextPosition = NextPosition,
					Direction = GetDirection(),
				});

				// velocity impulse
				var dir = GetDirection();

				float xImp = 0;
				float yImp = 0;

				if (dir == BulletDirection.DOWN) {
					yImp = 20;
				}
				if (dir == BulletDirection.UP) {
					yImp = -10;
				}
				if (dir == BulletDirection.LEFT) {
					xImp = -15;
				}
				if (dir == BulletDirection.RIGHT) {
					xImp = 15;
				}
				Velocity -= new Vector2(xImp, yImp);
			}
		}


		public override void ClientSideUpdate(IGameController gc, float dt) {
			
		}
		public bool IsDead() { return false; }
		public void Kill() {}
	}
}