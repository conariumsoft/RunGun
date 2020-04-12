using Microsoft.Xna.Framework;
using RunGun.Client;
using RunGun.Client.Input;

namespace RunGun.AndroidClient
{
	class AndroidChatManager : BaseChatSystem
	{

	}

	class AndroidClient : BaseClient {

		public AndroidClient() : base() {
			Chat = new AndroidChatManager();
			Input = new TouchInput();
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

			Input.Draw(SpriteBatch, GraphicsDevice);
		}
	}
}