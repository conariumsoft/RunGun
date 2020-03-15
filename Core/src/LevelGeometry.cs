using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core
{

	public enum GeometryColor
	{
		RED,
		GREEN,
		BLUE,
		YELLOW,
	}

	public class LevelGeometry : Entity
	{
		public Vector2 size;
		public Color color;
		

		public LevelGeometry() {

		}

		public LevelGeometry(Vector2 pos, Vector2 sz, Color col) {
			position = pos;
			size = sz;
			color = col;
		}

		public string Serialize() {
			return String.Format("{0} {1} {2} {3} {4} {5} {6}", position.X, position.Y, size.X, size.Y, color.R, color.G, color.B);
		}

		public Vector2 GetCenter() {
			return position + (size / 2);
		}

		public Vector2 GetDimensions() {
			return size / 2;
		}
	}
}
