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
		public List<string> InputMessageHistory;
		public string InputBuffer;
		public string InputDisplay;
		public int CursorPosition;
		public int HistoryPos;

		// workaround to grab arrow keys
		// LUL @MICROCOCK
		KeyListener listenLeft;
		KeyListener listenRight;
		//KeyListener listenUp;
		//KeyListener listenDown;

		//Action<string> onChat;

		double cursorBlinkClock;


		public override void OnKeyPress(Keys key) {
			base.OnKeyPress(key);
		}

		public override void OnTextInput(char inp, Keys key) {

			//base.OnTextInput(inp, key);
			if (IsClientTyping) {
				if (key == Keys.Enter) {
					OnClientSendMessage?.Invoke(InputBuffer);
					InputBuffer = "";
					IsClientTyping = false;
					CursorPosition = 0;
				} else if (key == Keys.Escape) {
					IsClientTyping = false;
					InputBuffer = "";
				} else if (key == Keys.Back) {
					if (CursorPosition > 0) {
						InputBuffer = InputBuffer.Remove(CursorPosition - 1, 1);
						CursorPosition--;
					}
				} else {
					InputBuffer = InputBuffer.Insert(CursorPosition, inp.ToString());
					CursorPosition++;
				}
			} else {
				if (key == Keys.T) {
					IsClientTyping = true;
				}
			}
		}

		private void Junk() { }

		public GLChatSystem() : base() {
			// use junk method for the release callback, since we don't care about it.
			listenLeft = new KeyListener(Keys.Left, OnLeftArrow, Junk);
			listenRight = new KeyListener(Keys.Right, OnRightArrow, Junk);
			//listenUp = new KeyListener(Keys.Up, OnUpArrow, Junk);
			//listenDown = new KeyListener(Keys.Down, OnDownArrow, Junk);

			InputBuffer = "";
			InputDisplay = "";
			InputMessageHistory = new List<string>();
			CursorPosition = 0;
			HistoryPos = 0;
			cursorBlinkClock = 0;
		}

		void OnLeftArrow() {
			if (!IsClientTyping) return;
			CursorPosition--;
			CursorPosition = Math.Max(CursorPosition, 0);
		}

		void OnRightArrow() {
			if (!IsClientTyping) return;
			CursorPosition++;
			CursorPosition = Math.Min(CursorPosition, InputBuffer.Length);
		}

		public override void Update(float delta) {
			base.Update(delta);
			//listenDown.Update();
			//listenUp.Update();
			listenLeft.Update();
			listenRight.Update();

			// add blinking cursor into string at proper position
			cursorBlinkClock += delta;

			InputDisplay = cursorBlinkClock % 1.0 > 0.5
				? (InputBuffer.Substring(0, CursorPosition)) + "|" + (InputBuffer.Substring(CursorPosition))
				: InputBuffer;
		}

		public override void Draw(SpriteBatch sb, GraphicsDevice graphics) {
			
			base.Draw(sb, graphics);
			int bottomScreen = 360 - 14;
			if (IsClientTyping) {
				TextRenderer.Print(sb, InputDisplay, new Vector2(4, bottomScreen), Color.LightGray);
			}
		}
	}
}
