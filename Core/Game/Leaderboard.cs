using System;
using System.Collections.Generic;
using System.Text;

namespace RunGun.Core.Game
{

	public class LeaderboardColumn {
		Dictionary<string, string> _list;

		public string DefaultValue {get;set;} = "0";

		public LeaderboardColumn() {
			_list = new Dictionary<string, string>();
		}

		public void AddEntry(string p) {
			_list.Add(p, DefaultValue);
		}

		public void AddEntry(string p, string val) {
			_list.Add(p, val);
		}

		public void RemoveEntry(string p) {
			_list.Remove(p);
		}

		public string GetEntry(string p) {
			return _list[p];
		}

		public void SetEntry(string p, string value) {
			_list[p] = value;
		}
	}
	class Leaderboard
	{
		Dictionary<string, LeaderboardColumn> _stats { get; set; }
		
		public void AddColumn(string key, LeaderboardColumn column) {
			_stats.Add(key, column);
		}

		public void RemoveColumn(string key) {
			_stats.Remove(key);
		}

		public LeaderboardColumn GetColumn(string key) {
			return _stats[key];
		}

	}
}
