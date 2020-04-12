using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client.Input;
using RunGun.Core.Game;
using RunGun.Core.Rendering;
using System;
using System.Collections.Generic;

namespace RunGun.Client
{
	public struct ChatMessage
	{
		public Color TextColor { get; set; }
		public string Text { get; set; }
	}

	public class BaseChatSystem : IChat
	{
		public bool IsClientTyping { get; set; }
		public Action<string> OnClientSendMessage { get; set; }
		public List<ChatMessage> Messages { get; }


		public BaseChatSystem() {
			Messages = new List<ChatMessage>();
		}

		public virtual void AddMessage(ChatMessage message) {
			Messages.Add(message);
		}
		public virtual void OnKeyPress(Keys key) {

		}

		public void EnterTyping() {
			IsClientTyping = true;
		}

		public virtual void OnTextInput(char inp, Keys key) { }

		public virtual void Update(float delta) { }
		
		public virtual void Draw(SpriteBatch sb, GraphicsDevice graphics) {
			int idx = Messages.Count;

			int bottomScreen = 360-27;
			foreach (ChatMessage message in Messages) {
				idx--;

				TextRenderer.Print(sb, message.Text, new Vector2(4, bottomScreen - (idx * 16)), message.TextColor);
			}
		}
	}
}
