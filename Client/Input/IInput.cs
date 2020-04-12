using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Client.Input
{

	

	public interface IChat : IGameSystem
	{
		public bool IsClientTyping { get; }
		public List<ChatMessage> Messages { get; }
		
		public Action<string> OnClientSendMessage { get; }

		public void AddMessage(ChatMessage mesage);
		public void OnTextInput(char inp, Keys key);
		public void OnKeyPress(Keys key);
		public void EnterTyping();
	}

	public interface IInput : IGameSystem
	{
		public bool MovingLeft { get; set; }
		public bool MovingRight { get; set; }
		public bool Jumping { get; set; }
		public bool Shooting { get; set; }
		public bool LookingDown { get; set; }
		public bool LookingUp { get; set; }

		public bool InChat { get; set; }

	}
}
