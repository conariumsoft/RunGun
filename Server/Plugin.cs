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

namespace RunGun.Server
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
		string PluginName { get; set; }
		string PluginAuthor { get; set; }
		string PluginPrefix { get; set; }
		string PluginVersion { get; set; }
		LuaFunction PluginInit { get; set; }
		LuaFunction ServerInit { get; set; }
		LuaFunction ServerStop { get; set; }
		LuaFunction OnConnectRequest { get; set; }
		IPluggableServer Server { get; }
	}
	class Plugin : IPlugin
	{
		public string PluginName { get; set; }
		public string PluginAuthor { get; set; }
		public string PluginPrefix { get; set; }
		public string PluginVersion { get; set; }
		public LuaFunction PluginInit { get; set; }
		public LuaFunction ServerInit { get; set; }
		public LuaFunction ServerStop { get; set; }
		public LuaFunction OnConnectRequest { get; set; }
		public IPluggableServer Server => throw new NotImplementedException();


		public Plugin() {
			
		}

		public void Init() {
			Logging.Out("Loaded " + PluginName + " by " + PluginAuthor);
		}
	}

	class PluginManager
	{
		List<Plugin> loadedPlugins { get; set; }
		public PluginManager() {
			loadedPlugins = new List<Plugin>();
		}
		public Plugin LoadPlugin(string rootFolderName) {
			using (Lua lua = new Lua()) {
				lua.LoadCLRPackage();
				lua.DoString(@"package.path = './plugins/" + rootFolderName + "/?.lua'");
				lua.DoFile(rootFolderName + "/metadata.lua");

				string root = lua["plugin.root"] as string;

				lua.DoFile(rootFolderName + "/init.lua");

				return new Plugin() {
					PluginName = lua["plugin.name"] as string,
					PluginAuthor = lua["plugin.author"] as string,
					PluginPrefix = lua["plugin.prefix"] as string,
					PluginVersion = lua["plugin.version"] as string,
					ServerInit = lua[root + ".OnServerStart"] as LuaFunction,
					ServerStop = lua[root + ".OnServerStop"] as LuaFunction,
					OnConnectRequest = lua[root + ".OnConnectRequest"] as LuaFunction,
				};
			}
		}

		private Plugin LoadTestPlugin() {
			using (Lua lua = new Lua()) {
				lua.LoadCLRPackage();
				
				lua.DoString(LuaSnippets.TestMetadata);

				string root = lua["plugin.root"] as string;

				lua.DoString(LuaSnippets.TestPluginDef);

				return new Plugin() {
					PluginName = lua["plugin.name"] as string,
					PluginAuthor = lua["plugin.author"] as string,
					PluginPrefix = lua["plugin.prefix"] as string,
					PluginVersion = lua["plugin.version"] as string,
					ServerInit = lua[root + ".OnServerStart"] as LuaFunction,
					ServerStop = lua[root + ".OnServerStop"] as LuaFunction,
				};
			}
		}

		public void LoadPlugins() {
			
			loadedPlugins.Add(LoadTestPlugin());
			string[] filenames = Directory.GetDirectories("plugins");
			foreach (var filename in filenames) {
				loadedPlugins.Add(LoadPlugin(filename));
			}

			foreach (var plugin in loadedPlugins) {
				plugin.Init();
			}

		}
	}
}