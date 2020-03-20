using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Server
{

	enum LogLevel
	{
		File,
		Console,
		Chat
	}

	public static class Chat
	{
		public static List<string> ChatHistory = new List<string>();

		//public static void HandleReceivedMessage(Client sender, string message) {

		//}

		public static void MessageAll(string outgoing) {

		}

		public static void MessagePlayer(string outgoing) {

		}

		public static void Log(string message, int level) {

		}
	}
}
