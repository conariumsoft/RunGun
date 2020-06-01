using RunGun.Client;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RunGun.Core.Networking;
using RunGun.Client.Input;

namespace RunGun.GLClient
{
	class GLClient : BaseClient 
	{
		public GLClient() : base() {
			Chat = new GLChatSystem();
			Input = new KeyboardInput();

			Window.TextInput += InputConnector;
			Window.AllowUserResizing = true;
			Window.AllowAltF4 = true;
		}

		private void InputConnector(object sender, TextInputEventArgs args) {
			Chat.OnTextInput(args.Character, args.Key);
		}

		public void OnPlayerSendChat(string str) {
			Client.Send(new CChat(str));
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
