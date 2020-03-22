using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RunGun.Core.Rendering
{
	public static class TextRenderer
	{
		static SpriteBatch spriteBatch;
		static SpriteFont defaultFont;

		public static void Initialize(SpriteBatch sb, ContentManager content) {
			defaultFont = content.Load<SpriteFont>("Font");
			spriteBatch = sb;
		}

		public static void Print(Color color, string text, Vector2 position) {
			spriteBatch.DrawString(defaultFont, text, position, color);
		}
	}
}
