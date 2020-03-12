using System;
using System.Collections.Generic;

namespace RunGun.Core.Bullshit
{
	public class EventHook {
		private List<Action> callbackList;

		public EventHook() {
			callbackList = new List<Action>();
		}

		public int Connect(Action method) {
			callbackList.Add(method);
			return callbackList.IndexOf(method);
		}

		public void Disconnect(int index) {
			callbackList.RemoveAt(index);
		}

		public void Call() {
			foreach (Action func in callbackList) {
				func();
			}
		}
	}
}
