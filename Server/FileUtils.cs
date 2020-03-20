using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace RunGun.Server.Utils
{
	public static class FileUtils
	{
		public static string ReadEmbedded(string filename) {
			var assembly = Assembly.GetExecutingAssembly();

			using (Stream stream = assembly.GetManifestResourceStream(filename))
			using (StreamReader reader = new StreamReader(stream)) {
				return reader.ReadToEnd();
			}
		}
	}
}
