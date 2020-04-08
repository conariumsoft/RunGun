using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Xna.Framework;
using RunGun.Client;

namespace RunGun.AndroidClient
{
	class AndroidChatManager : BaseChatSystem
	{

	}

	class AndroidClient : BaseClient {
		//public override ChatManager Chat { get; set; }

		public AndroidClient() : base() {
			Chat = new AndroidChatManager();

			graphicsDeviceManager.IsFullScreen = true;
			//graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
			//graphicsDeviceManager.ApplyChanges();

			Input = new InputManager(InputMode.TOUCH);
		}

		protected override void Initialize() {
			base.Initialize();
		}

		protected override void LoadContent() {
			base.LoadContent();
		}

		protected override void UnloadContent() {
			base.UnloadContent();
		}

		protected override void Update(GameTime gameTime) {
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime) {
			base.Draw(gameTime);
		}
	}
}