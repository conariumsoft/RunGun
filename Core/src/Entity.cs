//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using RunGun.Core.Physics;

namespace RunGun.Core
{
	public class Entity
	{
		public Vector2 Position { get; set; }

		public Entity() {
			Position = new Vector2();
		}

		public virtual void Update(double delta) { }
	}

	public interface ILiving
	{
		float Health { get; set; }
		float MaxHealth { get; set; }
		float Defense { get; set; }
		bool DestroyWhenDead { get; set; }

		//void Damage(float amount) { }
	}

	public class PhysicalEntity : Entity
	{
		public Vector2 NextPosition { get; set; }
		public Vector2 Velocity     { get; set; }
		public Vector2 BoundingBox  { get; set; }
		public bool    IsFrozen     { get; set; }
		public bool    IsFalling    { get; set; }
		public bool    ApplyGravity { get; set; }
		public float   Mass         { get; set; }

		public virtual void Physics(float step) {
			float x_friction = (Velocity.X * PhysicsProperties.FRICTION * step);
			float y_friction = (Velocity.Y * PhysicsProperties.FRICTION * step);

			Velocity -= new Vector2(x_friction, y_friction);

			if (IsFalling && ApplyGravity && Velocity.Y < PhysicsProperties.TERMINAL_VELOCITY)
				Velocity += new Vector2(0, PhysicsProperties.GRAVITY * Mass * step);

			NextPosition += (Velocity * step);
			Position = NextPosition;
		}
	}
}