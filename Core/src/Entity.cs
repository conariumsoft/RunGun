//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using RunGun.Core.Physics;
using System;

namespace RunGun.Core
{
	public class Entity
	{
		public static int idIncrement = 1;

		public int EntityID { get; set; }
		public Vector2 position { get; set; }

		public Entity() {
			position = new Vector2();

			EntityID = idIncrement;
			idIncrement++;
		}

		public Entity(int id) : base() {
			EntityID = id;
		}

		public virtual void Update(double delta) { }

		public Vector2 GetDrawPosition() {
			return new Vector2(0, 0);
		}
	}

	public class PhysicalEntity : Entity
	{
		public Vector2 nextPosition;
		public Vector2 velocity;
		public Vector2 boundingBox = new Vector2(16, 16);
		public bool isFrozen = false;
		public bool isFalling = true;
		public bool applyGravity = true;
		public float mass = 1;

		public Disk<Vector2> positionHistory = new Disk<Vector2>(256);
		public Disk<Vector2> velocityHistory = new Disk<Vector2>(256);

		public PhysicalEntity() : base() {
		
		}

		public PhysicalEntity(int id) : base(id) {

		}

		public virtual void Physics(float step) {
			float x_friction = 0;
			
			if (isFalling == false)
				x_friction = (velocity.X * PhysicsProperties.FRICTION * step);


			float y_friction = (velocity.Y * PhysicsProperties.FRICTION * step);

			velocity -= new Vector2(x_friction, y_friction);

			if (isFalling && applyGravity && velocity.Y < PhysicsProperties.TERMINAL_VELOCITY)
				velocity += new Vector2(0, PhysicsProperties.GRAVITY * mass * step);

			nextPosition += (velocity * step);
			position = nextPosition;
		}


		public virtual void OnCollide(Vector2 separation, Vector2 normal) {

		}
	}

	public class Barrel : PhysicalEntity
	{

	}
}