using NLua;
using NLua.Exceptions;
using RunGun.Core;
using RunGun.Core.Game;
using RunGun.Core.Utility;
using RunGun.Server.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RunGun.Server.Plugins
{
	static class LuaSnippets
	{
		
		public static string TestMetadata =
@"
plugin = {
	name = 'TemplatePlugin',
	prefix = 'TP',
	author = 'Joshua OLeary',
	website = 'getmeth.com',
	version = '1.0',
	description = 'testing plugin for the plugin system',
	root = 'MyPlugin',
	dependencies = {}
}
";
		public static string TestPluginDef =
@"
MyPlugin = {}

function MyPlugin:OnServerStart()
	
end

function MyPlugin:OnServerStop()
	
end

";
	}

	interface IPlugin
	{
		public bool PluginEnabled { get; set; }
		string PluginName { get; set; }
		string PluginAuthor { get; set; }
		string PluginPrefix { get; set; }
		string PluginVersion { get; set; }
		LuaFunction PluginInit { get; set; }
		LuaFunction ServerInit { get; set; }
		LuaFunction ServerStop { get; set; }
		LuaFunction OnConnectRequest { get; set; }
		IPluginAPI Server { get; }

		public FileInfo GetConfig();
		public void SaveConfig();

	}
	class Plugin : IPlugin
	{
		public bool PluginEnabled { get; set; }

		public string PluginName { get; set; }
		public string PluginAuthor { get; set; }
		public string PluginPrefix { get; set; }
		public string PluginVersion { get; set; }
		public LuaFunction PluginInit { get; set; }
		public LuaFunction ServerInit { get; set; }
		public LuaFunction ServerStop { get; set; }
		public LuaFunction OnConnectRequest { get; set; }
		public IPluginAPI Server => throw new NotImplementedException();

		public Plugin() {
			
		}

		public void Init() {
			Logging.Out("Loaded " + PluginName + " by " + PluginAuthor);
		}

		public FileInfo GetConfig() {
			throw new NotImplementedException();
		}

		public void SaveConfig() {
			throw new NotImplementedException();
		}
	}

}