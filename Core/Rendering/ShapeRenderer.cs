using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RunGun.Core.Rendering
{
	public static class ShapeRenderer
	{
		static Texture2D pixel;

		public static void Initialize(GraphicsDevice dev) {
			pixel = new Texture2D(dev, 1, 1);
			pixel.SetData<Color>(new Color[] { Color.White });
		}

		public static void Rect(SpriteBatch sb, Color color, Vector2 position, Vector2 size, float rotation = 0) {
			Rect(sb, color, (int)position.X, (int)position.Y, (int)size.X, (int)size.Y, rotation);
		}

		public static void Rect(SpriteBatch sb, Color color, int x, int y, int width, int height, float rotation = 0) {
			sb.Draw(
				pixel,
				new Rectangle(x, y, width, height),
				null,
				color, rotation, new Vector2(0, 0), SpriteEffects.None, 0
			);
		}

		public static void Line(SpriteBatch sb, Color color, Vector2 start, Vector2 end) {
			// see below for problems
			// https://gamedev.stackexchange.com/questions/44015/how-can-i-draw-a-simple-2d-line-in-xna-without-using-3d-primitives-and-shders
			Vector2 edge = end - start;
			float angle = (float)Math.Atan2(edge.Y, edge.X);
			sb.Draw(
				pixel, 
				new Rectangle( (int)start.X, (int)start.Y, (int)edge.Length(), 1),
				null, 
				color, angle, new Vector2(0, 0), SpriteEffects.None, 0
			);
		}
	}
}
