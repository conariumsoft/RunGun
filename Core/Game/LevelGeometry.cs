using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunGun.Core.Game;
using RunGun.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core
{
	public class LevelGeometry : IDrawableRG
	{
		public Vector2 Position;
		public Vector2 Size;
		public Color Color;

		public LevelGeometry(Vector2 pos, Vector2 sz, Color col) {
			Position = pos;
			Size = sz;
			Color = col;
		}

		
		public Vector2 GetCenter() {
			return Position + (Size / 2);
		}

		public Vector2 GetDimensions() {
			return Size / 2;
		}

		public void Draw(SpriteBatch sb) {
			ShapeRenderer.Rect(sb, Color, Position, Size);
		}
	}
}
