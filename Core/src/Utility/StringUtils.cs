using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RunGun.Core.Utils
{
	public static class StringUtils
	{
		public static string ReadUntil(string message, char sep) {

			if (message.Contains(sep)) {
				return message.Substring(0, message.IndexOf(sep));
			}
			return message;
		}

		public static string ReadAfter(string message, char sep) {
			if (message.Contains(sep)) {
				return message.Substring(message.IndexOf(sep) + 1);
			}
			return "";
		}
	}
}
