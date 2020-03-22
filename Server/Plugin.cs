using NLua;
using NLua.Exceptions;
using RunGun.Core;
using RunGun.Core.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RunGun.Server
{
	class PluginManager
	{
		static List<Plugin> loadedPlugins;

		public static event Action OnPluginLoad;
		public static event Action OnPluginUnload;
		public static event Action OnServerStart;
		public static event Action OnServerStop;
		public static event Action OnPlayerJoin;
		public static event Action OnPlayerLeave;

		// Event invokers to be called from server
		public static void CallOnPluginLoad() { OnPluginLoad?.Invoke(); }
		public static void CallOnPluginUnload() { OnPluginUnload?.Invoke(); }
		public static void CallOnServerStart() { OnServerStart?.Invoke(); }
		public static void CallOnServerStop() { OnServerStop?.Invoke(); }
		public static void CallOnPlayerJoin(Player p) { OnPlayerJoin?.Invoke(); }
		public static void CallOnPlayerLeave(Player p) { OnPlayerLeave?.Invoke(); }

		public static void LoadPlugins() {
			string[] filenames = Directory.GetDirectories("plugins");
			foreach (var filename in filenames) {
				Logging.Out("Loading " + filename + "...");
				using (StreamReader stream = new StreamReader(filename + "/init.lua")) {
					string read = stream.ReadToEnd();

					var plugin = new Plugin(filename, read);
					bool success = plugin.Execute(read);

					if (success) {
						loadedPlugins.Add(plugin);
						plugin.RetreiveCallbacks();

						OnPluginLoad += plugin.onPluginLoad.Callback;
						OnPluginUnload += plugin.onPluginUnload.Callback;
						OnServerStart += plugin.onServerStart.Callback;
						OnServerStop += plugin.onServerStop.Callback;
						OnPlayerJoin += plugin.onPlayerJoin.Callback;
						OnPlayerLeave += plugin.onPlayerLeave.Callback;
					}
				}
			}
		}
		public static void UnloadPlugins() {
			foreach (var plugin in loadedPlugins)
				plugin.Unload();
		}
	}

	struct LuaCallback
	{
		LuaFunction Function;
		public LuaCallback(LuaFunction f) {
			Function = f;
		}
		public void Callback() {
			Function?.Call();
		}
	}

	class Plugin
	{
		string rootFolder;
		Lua lua;

		public LuaCallback onPluginLoad;
		public LuaCallback onPluginUnload;
		public LuaCallback onServerStart;
		public LuaCallback onServerStop;
		public LuaCallback onPlayerJoin;
		public LuaCallback onPlayerLeave;

		public Plugin(string folderName, string data) {
			lua = new Lua();
			rootFolder = folderName;
			InitializeLuaVM();
		}
		~Plugin() {
			lua.Close();
			lua.Dispose();
		}

		public void Unload() {}

		public bool Execute(string body) {
			try {
				lua.DoString(body);
			}catch (LuaScriptException e) {
				// BITCH ABOUT ERROR
				return false;
			}
			return true;
		}

		public void RetreiveCallbacks() {
			onPluginLoad = new LuaCallback(lua["OnLoad"] as LuaFunction);
			onPluginUnload = new LuaCallback(lua["OnUnload"] as LuaFunction);
			onServerStart = new LuaCallback(lua["OnServerStart"] as LuaFunction);
			onServerStop = new LuaCallback(lua["OnServerStop"] as LuaFunction);
			onPlayerJoin = new LuaCallback(lua["OnPlayerJoin"] as LuaFunction);
			onPlayerLeave = new LuaCallback(lua["OnPlayerLeave"] as LuaFunction);
		}

		void InitializeLuaVM() {
			lua.DoString("package.path = './plugins/"+rootFolder+"/?.lua'");
		}
	}
}
