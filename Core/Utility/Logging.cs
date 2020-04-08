using System;

namespace RunGun.Core.Utility
{
	public static class Logging
	{

		public static void Out(string message) {
			Out(message, ConsoleColor.Cyan);
		}

		public static void Out(string message, ConsoleColor color) {
			//Console.BackgroundColor = ConsoleColor.DarkGray;
			Console.ForegroundColor = color;
			Console.Write("[S] [" + DateTime.Now.ToString("HH:mm:ss.f") + "] ");
			Console.ResetColor();
			Console.WriteLine(message);
		}

		public static void NetIn(string message) {
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.Write("[N] [" + DateTime.Now.ToString("HH:mm:ss.f") + "] ");
			Console.ResetColor();
			Console.WriteLine(message);
		}

		public static void NetOut(string message) {
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Write("[N] [" + DateTime.Now.ToString("HH:mm:ss.f") + "] ");
			Console.ResetColor();
			Console.WriteLine(message);
		}

		public static void Error(string error) {

		}
	}
}
