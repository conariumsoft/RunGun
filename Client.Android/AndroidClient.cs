using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using RunGun.Client;
using RunGun.Client.Input;

namespace RunGun.AndroidClient
{
	class AndroidChatManager : BaseChatSystem
	{

	}

	class AndroidClient : BaseClient {

		public AndroidClient() : base() {
			GraphicsDeviceManager.IsFullScreen = true;
			Chat = new AndroidChatManager();
			Input = new TouchInput();
			TouchPanel.DisplayOrientation = DisplayOrientation.LandscapeLeft;
			TouchPanel.DisplayWidth = GameConstants.BaseWidth;
			TouchPanel.DisplayHeight = GameConstants.BaseHeight;
			TouchPanel.EnableMouseTouchPoint = true;
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

		protected override void DrawGameLayer() {
			base.DrawGameLayer();
		}

		protected override void DrawUILayer() {
			base.DrawUILayer();
			Input.Draw(SpriteBatch, GraphicsDevice);
		}
	}
}