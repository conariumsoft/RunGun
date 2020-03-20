using Microsoft.Xna.Framework;
using RunGun.Core.Game.Guns;
using RunGun.Core.Physics;
using System;
using System.Text;

namespace RunGun.Core.Game
{
	public enum Looking { CENTER, UP, DOWN }
	public enum Facing { LEFT, RIGHT }

	public class Player : Entity
	{
		float walkAccelleration = 1000;
		float maxWalkspeed = 200;
		float jumpPower = 300;
		public bool destroyWhenDead = false;
		
		public bool moveLeft;
		public bool moveRight;
		public bool moveJump;
		public bool shooting;
		public bool lookUp;
		public bool lookDown;

		public double bulletTimer;

		public string UserNickname { get; set; }
		public Guid UserGUID { get; set; }

		public Color color;
		public BaseGun equippedGun;

		public float health = 100;
		public float maxHealth = 100;
		public float defense = 0;
		
		public Looking looking;
		public Facing facing;

		public Player() {
			Random r = new Random();
			Position = new Vector2(64, 64);
			NextPosition = new Vector2(64, 64);
			boundingBox = new Vector2(16, 16);
			color = new Color(r.Next(1, 255), r.Next(1, 255), r.Next(1, 255));
			bulletTimer = 0;
		}

		public static Player Decode(byte[] packet) {
			short entityID = BitConverter.ToInt16(packet, 0);
			
			byte r = packet[2];
			byte g = packet[3];
			byte b = packet[4];
			string nickname = Encoding.ASCII.GetString(packet, 5, packet.Length-5);
			return new Player() {
				EntityID = entityID,
				UserNickname = nickname,
				color = new Color(r, g, b)
			};
		}

		public new Vector2 GetDrawPosition() {
			return Position - (boundingBox);
		}

		public override void Update(double delta) {

			bulletTimer -= delta;

			
		}

		public override void OnCollide(Vector2 separation, Vector2 normal) {
			base.OnCollide(separation, normal);
			if (normal.Y == -1) {
				IsFalling = false;
				Velocity.Y = 0;

				if (!moveLeft && !moveRight) {
					Velocity = new Vector2(Velocity.X * 0.9f, Velocity.Y);
				}
			}

			if (normal.Y == 1) {
				Velocity.Y = (-Velocity.Y) * 0.3f;
			}

			if (normal.X != 0) {
				Velocity.X = 0;
			}
		}

		public override void Physics(float step) {
			base.Physics(step);

			float x_thrust = 0;
			float y_thrust = 0;

			if (moveLeft && Velocity.X > -maxWalkspeed) {
				x_thrust = (-walkAccelleration * step);
			}
			if (moveRight && Velocity.X < maxWalkspeed) {
				x_thrust += (walkAccelleration * step);
			}
			if (moveJump && IsFalling == false) {
				IsFalling = true;
				y_thrust = -(jumpPower * Mass);
			}

			Velocity += new Vector2(x_thrust, y_thrust);

		}
	}
}