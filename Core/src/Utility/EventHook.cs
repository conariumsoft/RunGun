using RunGun.Core.Networking;
using System;
using System.Collections.Generic;

namespace RunGun.Core.Utility
{

	public abstract class Event
	{
		public static bool cancellable = false;
		bool cancelled;


		public Event() {

		}

		public void SetCancelled(bool value) {
			if (cancellable)
				cancelled = value;
		}
	}

	// todo: move somewhere else
	public class NetworkEvent : Event
	{
		public ClientCommand cmd;
		public List<string> args;

		public NetworkEvent(ClientCommand command, List<string> arguments) {
			cmd = command;
			args = arguments;
		}

		public ClientCommand GetCommand() { return cmd; }
		public List<string> GetArguments() { return args; }

	}


	// todo: move somewhere else
	

	public class EventHook<T> where T : Event
	{
		private List<Action<T>> callbacks;

		public EventHook() {
			callbacks = new List<Action<T>>();
		}
		
		public void Connect(Action<T> callback) {
			callbacks.Add(callback);
		}

		public void Call(T e) {
			foreach (var callback in callbacks) {
				callback.Invoke(e);
			}
		}
	}
	// calling it "Narrow" because you can narrow down one specific callback.
	// drawback: only one callback per key
	// will make a new class if this ever becomes issue
	public class NarrowEventHook<D, T> where T : Event
	{
		private Dictionary<D, Action<T>> callbacks;

		public NarrowEventHook() {
			callbacks = new Dictionary<D, Action<T>>();
		}
		
		public void Connect(D specifier, Action<T> callback) {
			callbacks.Add(specifier, callback);
		}

		public void Call(D specifier, T e) {
			foreach (KeyValuePair<D, Action<T>> kvp in callbacks) {
				if (specifier.Equals(kvp.Key)) {
					kvp.Value.Invoke(e);
				}
			}
		}
	}
}
