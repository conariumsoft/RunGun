using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RunGun.Core.Rendering
{
	public static class ShapeRenderer
	{
		static Texture2D pixel;

		static SpriteBatch spriteBatch;

		public static void Initialize(GraphicsDevice dev, SpriteBatch sb) {
			pixel = new Texture2D(dev, 1, 1);
			pixel.SetData<Color>(new Color[] { Color.White });
			spriteBatch = sb;
		}

		public static void DrawRect(Color color, Vector2 position, Vector2 size) {
			DrawRect(color, (int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
		}

		public static void DrawRect(Color color, int x, int y, int width, int height) {
			spriteBatch.Draw(
				pixel,
				new Rectangle(x, y, width, height),
				null,
				color, 0, new Vector2(0, 0), SpriteEffects.None, 0
			);
		}

		public static void DrawLine(Color color, Vector2 start, Vector2 end) {
			// see below for problems
			// https://gamedev.stackexchange.com/questions/44015/how-can-i-draw-a-simple-2d-line-in-xna-without-using-3d-primitives-and-shders
			Vector2 edge = end - start;
			float angle = (float)Math.Atan2(edge.Y, edge.X);
			spriteBatch.Draw(
				pixel, 
				new Rectangle( (int)start.X, (int)start.Y, (int)edge.Length(), 1),
				null, 
				color, angle, new Vector2(0, 0), SpriteEffects.None, 0
			);
		}
	}
}
