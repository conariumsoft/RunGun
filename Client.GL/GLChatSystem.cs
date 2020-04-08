using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client;
using RunGun.Client.Misc;
using RunGun.Core.Rendering;
using System;
using System.Collections.Generic;

namespace RunGun.GLClient
{
	public class GLChatSystem : BaseChatSystem
	{
		public bool isTypingMessage;
		public List<string> typedMessageHistory;
		public string typingMessageBuffer;
		public string typingMessageDisplay;
		public int cursorPos;
		public int historyPos;

		// workaround to grab arrow keys
		// LUL @MICROCOCK
		KeyListener listenLeft;
		KeyListener listenRight;
		KeyListener listenUp;
		KeyListener listenDown;

		Action<string> onChat;

		double cursorBlinkClock;

		public GLChatSystem(Action<string> onChatCallback) : base() {
			// use junk method for the release callback, since we don't care about it.
			listenLeft = new KeyListener(Keys.Left, OnLeftArrow, Junk);
			listenRight = new KeyListener(Keys.Right, OnRightArrow, Junk);
			listenUp = new KeyListener(Keys.Up, OnUpArrow, Junk);
			listenDown = new KeyListener(Keys.Down, OnDownArrow, Junk);

			onChat = onChatCallback;

			typingMessageBuffer = "";
			typingMessageDisplay = "";
			typedMessageHistory = new List<string>();
			cursorPos = 0;
			historyPos = 0;
			cursorBlinkClock = 0;
		}

		void OnLeftArrow() {
			if (!isTypingMessage) return;
			cursorPos--;
			cursorPos = Math.Max(cursorPos, 0);
		}

		void OnRightArrow() {
			if (!isTypingMessage) return;
			cursorPos++;
			cursorPos = Math.Min(cursorPos, typingMessageBuffer.Length);
		}

		void OnUpArrow() { }

		void OnDownArrow() { }

		private void Junk() { }

		public override void ReceivedMessage(string message) {
			AddMessage(new ChatMessage { Text = message, TextColor = Color.White });
		}

		public void OnSendMessage(string message) {
			onChat(message);
		}

		public void OnTextInput(object sender, TextInputEventArgs args) {
			char inp = args.Character;
			Keys key = args.Key;

			if (isTypingMessage) {
				if (key == Keys.Enter) {
					OnSendMessage(typingMessageBuffer);
					typingMessageBuffer = "";
					isTypingMessage = false;
					cursorPos = 0;
				} else if (key == Keys.Escape) {
					isTypingMessage = false;
					typingMessageBuffer = "";
				} else if (key == Keys.Back) {
					if (cursorPos > 0) {
						typingMessageBuffer = typingMessageBuffer.Remove(cursorPos - 1, 1);
						cursorPos--;
					}
				} else {
					typingMessageBuffer = typingMessageBuffer.Insert(cursorPos, inp.ToString());
					cursorPos++;
				}
			} else {
				if (key == Keys.T) {
					isTypingMessage = true;
				}
			}
		}

		public override void Update(float delta) {
			listenDown.Update();
			listenUp.Update();
			listenLeft.Update();
			listenRight.Update();

			cursorBlinkClock += delta;

			if (cursorBlinkClock % 1.0 > 0.5) {
				typingMessageDisplay = (typingMessageBuffer.Substring(0, cursorPos)) + "|" + (typingMessageBuffer.Substring(cursorPos));
			} else {
				typingMessageDisplay = typingMessageBuffer;
			}
		}

		public override void Draw(SpriteBatch sb) {
			base.Draw(sb);

			if (isTypingMessage) {
				TextRenderer.Print(sb, typingMessageDisplay, new Vector2(4, 570), Color.LightGray);
			}
		}
	}
}
