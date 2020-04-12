using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Utility
{
	public interface ITask
	{

		public Action Task { get; }

		public void Update(float delta);
	}

	public class IntervalTask : ITask
	{
		public float Interval { get; }
		public Action Task { get; }
		public bool PreserveIntervalOvertime { get; }

		private float timer;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="interval">Interval (times per second) to run task.</param>
		/// <param name="action">Method to be ran</param>
		/// <param name="preserve">Preserve any overtime between intervals.</param>
		public IntervalTask(float interval, Action action, bool preserve = false) {
			Interval = interval;
			Task = action;
			PreserveIntervalOvertime = preserve;
		}

		public void Update(float dt) {
			timer += dt;

			if (timer >= (1.0f / Interval) ) {
				if (PreserveIntervalOvertime)
					timer -= Interval;
				else
					timer = 0;

				Task.Invoke();
			}
		}
	}

	public static class TaskManager
	{

		public static List<ITask> Tasks = new List<ITask>();

		public static void Register(ITask task) {
			Tasks.Add(task);
		}

		public static void Update(float dt) {
			foreach (var task in Tasks) {
				task.Update(dt);
			}
		}
	}
}
