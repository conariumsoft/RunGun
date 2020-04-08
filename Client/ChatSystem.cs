using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

	public interface IChatSystem
	{
		public void Update(float delta);
		public void Draw(SpriteBatch sb);
		public void AddMessage(ChatMessage message);
	}

	public class BaseChatSystem : IDrawableRG, IChatSystem
	{
		public bool IsLocalTypingMessage { get; set; }
		public Action<string> OnLocalMessageSent { get; set; }
		public List<ChatMessage> Messages { get; set; }


		public BaseChatSystem() {
			Messages = new List<ChatMessage>();
		}
		public virtual void AddMessage(ChatMessage message) {
			Messages.Add(message);
		}

		public virtual void ReceivedMessage(string message) { }

		public virtual void Update(float delta) { }
		
		public virtual void Draw(SpriteBatch sb) {
			int idx = Messages.Count;
			foreach (ChatMessage message in Messages) {
				idx--;
				TextRenderer.Print(sb, message.Text, new Vector2(4, 550 - (idx * 16)), message.TextColor);
			}
		}

		public void OnInput(char input, Keys key) {
			throw new NotImplementedException();
		}
	}
}
