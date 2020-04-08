using RunGun.Client;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RunGun.GLClient
{
	class GLClient : BaseClient 
	{

		public GLClient() : base() {
			Chat = new GLChatSystem(OnPlayerSendChat);
			Input = new InputManager(InputMode.KEYBOARD);

			//Window.TextInput += Chat.OnTextInput;
			Window.AllowUserResizing = true;
			Window.AllowAltF4 = true;
		}

		public void OnPlayerSendChat(string str) {
			// TODO: fixxx
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
