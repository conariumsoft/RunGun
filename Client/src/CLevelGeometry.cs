using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RunGun.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunGun.Client
{
	class GeometrySprite
	{

		Texture2D texture;
		Color[] colordata;

		public GeometrySprite(GraphicsDevice graphicsDev, int width, int height, Color col) {
			texture = new Texture2D(graphicsDev, width, height);

			colordata = new Color[width * height];

			for (int i = 0; i < (width*height); i++) {
				colordata[i] = col;
			}

			texture.SetData(colordata);
		}


		public void Draw(SpriteBatch spriteBatch, Vector2 position) {
			spriteBatch.Draw(texture, position, Color.White);
		}

	}

	class CLevelGeometry : LevelGeometry
	{
		GeometrySprite sprite;

		public CLevelGeometry(GraphicsDevice graphicsDev, Vector2 pos, Vector2 sz, Color col) {
			sprite = new GeometrySprite(graphicsDev, (int)sz.X, (int)sz.Y, col);
			position = pos;
			size = sz;
			color = col;

		}

		public void Draw(SpriteBatch spriteBatch) {
			sprite.Draw(spriteBatch, position);
		}
	}
}
