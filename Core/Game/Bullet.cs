using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Game
{
	enum BulletDirection
	{
		UP, DOWN, LEFT, RIGHT
	}
	class Bullet : Entity, IEntityCollidable
	{
		Entity bulletCreator;
		BulletDirection direction;
		public Bullet(Entity creator) : base() {
			bulletCreator = creator;
		}

		public override void Update(double delta) {
			base.Update(delta);
		}

		public override void OnCollide(Vector2 separation, Vector2 normal) {
			base.OnCollide(separation, normal);
			dead = true;
		}

		public override void Physics(float step) {
			//base.Physics(step);
			// TODO: implement bullet penetration
			// override base physics, we don't 
			Position = NextPosition;
			NextPosition = Velocity * step;
		}

		public void OnEntityCollide(Vector2 sep, Vector2 normal, Entity victim) {
			if (!victim.Equals(bulletCreator)) {
				dead = true;
			}
		}
	}
}
