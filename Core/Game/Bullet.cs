using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunGun.Core.Rendering;
using System;

namespace RunGun.Core.Game
{
	public enum BulletDirection : byte
	{
		UP, DOWN, LEFT, RIGHT
	}
	public class Bullet : PhysicalEntity, IEntityCollidable, IPhysical, IEntity
	{
		float bulletSpeed = 250f; //pixel/second?

		public Vector2 BoundingBox { get; } = new Vector2(4, 4);
		public short CreatorID { get; set; }
		public BulletDirection Direction { get; set; }
		public Bullet() {
			Mass = 0;
		}

		public override void Update(float delta) {
			base.Update(delta);
			if (Age > 5.0f)
				Remove = true;
		}

		public void OnCollide(Vector2 separation, Vector2 normal) {
			Remove = true;
		}

		public override void Physics(float step) {
			base.Physics(step);
			
			switch(Direction) {
				case BulletDirection.LEFT:
					Velocity = new Vector2(-1, 0) * bulletSpeed;
					break;
				case BulletDirection.RIGHT:
					Velocity = new Vector2(1, 0) * bulletSpeed;
					break;
				case BulletDirection.DOWN:
					Velocity = new Vector2(0, 1) * bulletSpeed;
					break;

				case BulletDirection.UP:
					Velocity = new Vector2(0, -1)*bulletSpeed;
					break;
			}

			NextPosition += (Velocity * step);
			Position = NextPosition;
		}

		public void OnEntityCollide(Vector2 sep, Vector2 normal, ICollidable victim) {
			if (!(victim is Bullet) && victim.EntityID != CreatorID) {
				if (victim is ILiving living) {
					living.Health -= 10;
					Remove = true;
				}
			}
		}

		public override void Draw(SpriteBatch sb) {
			base.Draw(sb);
			ShapeRenderer.Rect(sb, Color.Red, Position, BoundingBox);
		}
	}
}
