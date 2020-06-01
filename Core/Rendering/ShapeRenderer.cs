using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RunGun.Core.Rendering
{
	public static class ShapeRenderer
	{
		static Texture2D pixel;
		static GraphicsDevice GraphicsDev;

		public static void Initialize(GraphicsDevice dev) {
			GraphicsDev = dev;
			pixel = new Texture2D(dev, 1, 1);
			pixel.SetData<Color>(new Color[] { Color.White });
		}

		public static void OutlineRect(SpriteBatch sb, Color color, Vector2 position, Vector2 size) {
			Line(sb, color, position, position + new Vector2(0, size.Y), 2);
			Line(sb, color, position, position + new Vector2(size.X, 0), 2);
			Line(sb, color, position + new Vector2(size.X, 0), position + size, 2);
			Line(sb, color, position + new Vector2(0, size.Y), position + new Vector2(size.X, size.Y), 2);
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
			// retardretardretardretardretardretard
		}

		public static void Line(this SpriteBatch spriteBatch, Color color, Vector2 point1, Vector2 point2, float thickness = 1f) {
			float distance = Vector2.Distance(point1, point2);
			float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);

			float expanded = (float)Math.Floor(angle * Math.PI);
			float backDown = expanded / (float)Math.PI;

			Line(spriteBatch, color, point1, distance, angle, thickness);
		}

		public static void Line(this SpriteBatch sb, Color color, Vector2 point, float length, float angle, float thickness = 1f) {
			Vector2 origin = new Vector2(0f, 0.5f);
			Vector2 scale = new Vector2(length, thickness);
			sb.Draw(pixel, point, null, color, angle, origin, scale, SpriteEffects.None, 0);
		}
	}
}
