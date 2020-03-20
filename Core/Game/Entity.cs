//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunGun.Core.Physics;
using System;

namespace RunGun.Core.Game
{
	public class Entity
	{
		public Vector2 boundingBox = new Vector2(16, 16);
		public short EntityID { get; set; }
		public Vector2 Position;
		public Vector2 NextPosition;
		public Vector2 Velocity;
		public int Health { get; set; }

		public bool IsFrozen = false;
		public bool IsFalling = true;
		public bool ApplyGravity = true;
		public float Mass = 1;

		public Disk<Vector2> positionHistory = new Disk<Vector2>(256);
		public Disk<Vector2> velocityHistory = new Disk<Vector2>(256);

		public bool dead;

		public Entity() {
			Position = new Vector2();
			dead = false;
		}

		public virtual void Physics(float step) {
			float x_friction = 0;

			if (IsFalling == false)
				x_friction = (Velocity.X * PhysicsProperties.FRICTION * step);


			float y_friction = (Velocity.Y * PhysicsProperties.FRICTION * step);

			Velocity -= new Vector2(x_friction, y_friction);

			if (IsFalling && ApplyGravity && Velocity.Y < PhysicsProperties.TERMINAL_VELOCITY)
				Velocity += new Vector2(0, PhysicsProperties.GRAVITY * Mass * step);

			NextPosition += (Velocity * step);
			Position = NextPosition;
		}

		public virtual void OnCollide(Vector2 separation, Vector2 normal) {

		}

		public virtual void Update(double delta) { }

		public virtual void ServerUpdate(double delta) { }

		public virtual void ClientUpdate(double delta) { }

		public Vector2 GetDrawPosition() {
			return new Vector2(0, 0);
		}

		public void Draw(SpriteBatch batch) {

		}
	}

	public interface IEntityCollidable
	{
		void OnEntityCollide(Vector2 sep, Vector2 normal, Entity victim);
	}

	public class Barrel : Entity
	{

	}
}