using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RunGun.Client;
using RunGun.Client.Misc;
using RunGun.Core.Rendering;
using System;
using System.Collections.Generic;

namespace RunGun.GLClient
{
	class GLChatSystem : BaseChatSystem
	{
		public List<ChatMessage> messages;
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

		public GLChatSystem(Action<string> onChatCallback) {
			// use junk method for the release callback, since we don't care about it.
			listenLeft = new KeyListener(Keys.Left, OnLeftArrow, Junk);
			listenRight = new KeyListener(Keys.Right, OnRightArrow, Junk);
			listenUp = new KeyListener(Keys.Up, OnUpArrow, Junk);
			listenDown = new KeyListener(Keys.Down, OnDownArrow, Junk);

			onChat = onChatCallback;

			messages = new List<ChatMessage>();
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

		public void AddMessage(ChatMessage message) {
			messages.Add(message);
		}

		public override void ReceivedMessage(string message) {
			AddMessage(new ChatMessage { text = message, textColor = Color.White });
		}

		public void OnSendMessage(string message) {
			//Console.WriteLine("FUCK " + message);
			//AddMessage(new ChatMessage { text = message, textColor = Color.White });
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

		public override void Draw() {
			int range = messages.Count;

			int idx = messages.Count;
			foreach (ChatMessage message in messages) {
				idx--;
				//spriteBatch.DrawString(Fonts.GameFont, message.text, new Vector2(4, 550 - (idx * 16)), message.textColor);
				TextRenderer.Print(message.textColor, message.text, new Vector2(4, 550 - (idx * 16)));
			}

			if (isTypingMessage) {
				//spriteBatch.DrawString(Fonts.GameFont, typingMessageDisplay, new Vector2(4, 570), Color.LightGray);
				TextRenderer.Print(Color.LightGray, typingMessageDisplay, new Vector2(4, 570));
			}
		}
	}
}
