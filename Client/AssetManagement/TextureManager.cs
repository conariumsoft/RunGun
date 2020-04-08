using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Client.AssetManagement
{
	public static class TextureManager
	{
		#region EssentialTextureDefinitions
		public static Texture2D ErrorTexture { get; private set; }
		#endregion
		#region DefineTextureNamesHere


		public static Texture2D TestTexture { get; private set; }

		#endregion

		public static void LoadTextures(ContentManager content) {
			LoadEssentialTextures(content); // shit that is required for the game to boot up.

			#region CallTextureLoadersHere

			TestTexture = LoadTexture(content, "Test");

			#endregion
		}

		public static void UnloadTextures() {
			ErrorTexture.Dispose();
		}

		#region DontFuckWithThisStuffGuys
		// loads the texture, or returns a default.
		private static void LoadEssentialTextures(ContentManager content) {
			ErrorTexture = LoadTexture(content, "ErrorTexture");
		}

		private static Texture2D LoadTexture(ContentManager content, string name) {
			Texture2D tex;
			try {
				tex = content.Load<Texture2D>(name);
			} catch (ContentLoadException ex) {
				Console.WriteLine("Failed to load texture [" + name + "] info:" + ex.Message);
				tex = ErrorTexture;
			}
			return tex;
		}
		#endregion
	}
}
