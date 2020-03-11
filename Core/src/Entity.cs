//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using RunGun.Core.Physics;

namespace RunGun.Core
{
	public class Entity
	{
		public Vector2 position { get; set; }

		public Entity() {
			position = new Vector2();
		}

		public virtual void Update(double delta) { }
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
}