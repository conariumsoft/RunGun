using Microsoft.Xna.Framework;
using RunGun.Core.Game;

namespace RunGun.Client
{
	public struct ChatMessage
	{
		public Color textColor;
		public string text;
	}

	public class BaseChatSystem : IDrawableRG
	{
		public virtual void Update(float delta) { }
		public virtual void Draw() { }
		public virtual void OnTextInput() { }
		public virtual void ReceivedMessage(string message) { }
	}
}
