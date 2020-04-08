using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RunGun.Core.Rendering
{
	public static class TextRenderer
	{
		static SpriteFont defaultFont;

		public static void Initialize(ContentManager content) {
			defaultFont = content.Load<SpriteFont>("Font");
		}

		public static void Print(SpriteBatch sb, string text, Vector2 position, Color color) {
			Print(sb, defaultFont, text, position, color);
		}

		public static void Print(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color color) {
			sb.DrawString(font, text, position, color);
		}
	}
}
