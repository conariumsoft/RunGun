using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunGun.Core.Game;
using RunGun.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core
{
	public interface IMapInstance
	{

	}
	public interface ILevelGeometry : IRenderComponent
	{
		public Vector2 Position { get; set; }
		public Vector2 Size { get; set; }
		public Color Color { get; set; }
	}

	[Serializable]
	public class LevelGeometry : ILevelGeometry, IRenderComponent
	{
		public Vector2 Position { get; set; }
		public Vector2 Size { get; set; }
		public Color Color { get; set; }

		public LevelGeometry() { }
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
			ShapeRenderer.Rect(sb, Color.Black, Position + new Vector2(4, 4), Size - new Vector2(8, 8));
		}
	}
}
