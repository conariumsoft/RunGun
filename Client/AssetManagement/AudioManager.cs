using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Client.AssetManagement
{
	public static class AudioManager 
	{

		public static Song TestSong;
		public static SoundEffect TestSFX;


		private static void LoadSoundEffectFiles(ContentManager content) {

		}

		private static void LoadSongFiles(ContentManager content) {

		}

		public static void LoadAudioFiles(ContentManager content) {
			LoadSongFiles(content);
			LoadSoundEffectFiles(content);
		}

		public static void UnloadAudioFiles() {

		}

		private static void LoadSoundEffect() {

		}

		private static void LoadSong() {

		}
	}
}
