using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RunGun.Core.Game
{
	enum BulletDirection
	{
		UP, DOWN, LEFT, RIGHT
	}
	class Bullet : Entity, IEntityCollidable
	{
		Entity bulletCreator;
		float bulletSpeed = 200; //pixel/second?

		public Bullet(Entity creator, Vector2 origin, Vector2 direction) : base() {
			bulletCreator = creator;

			Position = origin;
			NextPosition = origin;
			Velocity = direction * bulletSpeed;
		}

		public override void Update(float delta) {
			base.Update(delta);
		}

		public override void OnCollide(Vector2 separation, Vector2 normal) {
			base.OnCollide(separation, normal);
			dead = true;
		}

		public override void Physics(float step) {
			//base.Physics(step);
			// TODO: implement bullet penetration
			Position = NextPosition;
			NextPosition = Velocity * step;
		}

		public void OnEntityCollide(Vector2 sep, Vector2 normal, Entity victim) {
			if (!victim.Equals(bulletCreator)) {
				dead = true;
			}
		}

		public override void Draw() {
			base.Draw();
		}
	}
}
