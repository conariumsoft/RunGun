﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Physics
{
	public class CollisionSolver
	{

		public static bool CheckAABB(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB) {
			float absDistanceX = Math.Abs(posA.X - posB.X);
			float absDistanceY = Math.Abs(posA.Y - posB.Y);

			float sumWidth = sizeA.X + sizeB.X;
			float sumHeight = sizeA.Y + sizeB.Y;

			if (absDistanceY >= sumHeight || absDistanceX >= sumWidth) {
				return false;
			}
			return true;
		}

		public static Vector2 GetSeparationAABB(Vector2 posA, Vector2 sizeA, Vector2 posB, Vector2 sizeB) {
			float distanceX = posA.X - posB.X;
			float distanceY = posA.Y - posB.Y;

			float absDistanceX = Math.Abs(distanceX);
			float absDistanceY = Math.Abs(distanceY);

			float sumWidth = sizeA.X + sizeB.X;
			float sumHeight = sizeA.Y + sizeB.Y;

			float sx = sumWidth - absDistanceX;
			float sy = sumHeight - absDistanceY;


			if (sx > sy) {
				if (sy > 0) {
					sx = 0;
				}
			} else {
				if (sx > 0) {
					sy = 0;
				}
			}

			if (distanceX < 0) {
				sx = -sx;
			}
			if (distanceY < 0) {
				sy = -sy;
			}

			return new Vector2(sx, sy);
		}

		public static Vector2 GetNormalAABB(Vector2 separation, Vector2 velocity) {
			float d = (float)Math.Sqrt(separation.X * separation.X + separation.Y * separation.Y);

			float nx = separation.X / d;
			float ny = separation.Y / d;

			float ps = velocity.X * nx + velocity.Y * ny;

			// maybe needed?
			if (ps <= 0) {
				return new Vector2(nx, ny);
			}
			return new Vector2(0, 0);

		}
	}
}