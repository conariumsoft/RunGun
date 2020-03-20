using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace RunGun.Core.Utility
{
	public static class Cryptography
	{
		static HashAlgorithm sha256 = SHA256.Create();

		public static byte[] GetHash(string input) {
			return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
		}

		public static string GetHashString(string input) {
			StringBuilder sb = new StringBuilder();

			foreach (byte b in GetHash(input)) {
				sb.Append(b.ToString("X2"));
			}

			return sb.ToString();
		}
	}
}
