using NLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RunGun.Server.Plugins
{
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
