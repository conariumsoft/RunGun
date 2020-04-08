//using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunGun.Core.Generic;
using RunGun.Core.Physics;

namespace RunGun.Core.Game
{

	public interface IEntity
	{
		short EntityID { get; set; }
		float Age { get; }
		Vector2 Position { get; set; }
		bool IsActive { get; set; }
		bool Remove { get; set; }

		void Update(float dt);
		void ServerSideUpdate(IGameController gc, float dt);
		void ClientSideUpdate(IGameController gc, float dt);
	}

	public interface INetworkReplicated
	{

	}

	public interface ILiving : IEntity
	{
		int Health { get; set; }

		bool IsDead();
		void Kill();
	}

	public interface IPhysical : IEntity
	{
		Vector2 NextPosition { get; set; }
		Vector2 Velocity { get; set; }
		float Mass { get; }
		bool IsFalling { get; set; }
		bool ApplyGravity { get; }

		void Physics(float step);
	}

	public interface ICollidable : IPhysical
	{
		Vector2 BoundingBox { get; }
		void OnCollide(Vector2 separation, Vector2 normal);
	}

	public interface IEntityCollidable : ICollidable
	{
		void OnEntityCollide(Vector2 sep, Vector2 normal, ICollidable victim);
	}

	public abstract class Entity : IEntity, IUpdateableRG, IDrawableRG
	{
		public bool Remove { get; set; }
		public short EntityID { get; set; }
		public float Age { get; set; }
		public Vector2 Position { get; set; }
		public bool IsActive { get; set; }

		public Entity() {}
		public virtual void Update(float delta) {
			Age += delta;
		}
		public virtual void ClientSideUpdate(IGameController gc, float dt) {}
		public virtual void ServerSideUpdate(IGameController gc, float dt) {}
		public virtual void Draw(SpriteBatch sb) {
			//TextRenderer.Print(Color.White, Position.ToString(), Position);
		}
	}

	public abstract class PhysicalEntity : Entity, IPhysical {
		public CircularArray<Vector2> positionHistory = new CircularArray<Vector2>(256);
		public CircularArray<Vector2> velocityHistory = new CircularArray<Vector2>(256);

		public Vector2 NextPosition { get; set; }
		public Vector2 Velocity { get; set; }

		public float Mass { get; protected set; } = 1;

		public bool IsFalling { get; set; }

		public bool ApplyGravity { get;} = true;

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

	}
}