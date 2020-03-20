using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RunGun.Client.Misc;
using RunGun.Core;
using System;
using System.Collections.Generic;

namespace RunGun.Client
{
	public struct ChatMessage
	{
		public Color textColor;
		public string text;
	}

	class ChatSystem
	{

		List<ChatMessage> messages;
		bool isTypingMessage;
		List<string> typedMessageHistory;
		string typingMessageBuffer;
		string typingMessageDisplay;
		int cursorPos;
		int historyPos;

		// workaround to grab arrow keys
		// LUL @MICROCOCK
		KeyListener listenLeft;
		KeyListener listenRight;
		KeyListener listenUp;
		KeyListener listenDown;

		Action<string> onChat;

		double cursorBlinkClock;

		public ChatSystem(Action<string> onChatCallback) {
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

		public void ReceivedMessage(string message) {
			AddMessage(new ChatMessage { text = message, textColor = Color.White });
		}

		public void OnSendMessage(string message) {
			//Console.WriteLine("FUCK " + message);
			//AddMessage(new ChatMessage { text = message, textColor = Color.White });
			onChat(message);
		}

		/*public void OnTextInput(object sender, TextInputEventArgs args) {
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
		}*/

		public void Update(double delta) {
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

		public void Draw(SpriteBatch spriteBatch) {
			int range = messages.Count;

			int idx = messages.Count;
			foreach (ChatMessage message in messages) {
				idx--;
				spriteBatch.DrawString(ClientMain.font, message.text, new Vector2(4, 550-(idx * 16)), message.textColor);
			}

			if (isTypingMessage) {
				spriteBatch.DrawString(ClientMain.font, typingMessageDisplay, new Vector2(4, 570), Color.LightGray);
			}
		}
	}
}
