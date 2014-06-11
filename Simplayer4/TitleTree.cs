using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Simplayer4 {
	public class TitleTree {
		static string SearchTag = "";
		static KeyValuePair<string, int> KvpTag;

		public static int GetPositionByHeader(char c) {
			if (ListTag == null || ListTag.Count == 0) { return -1; }
			SearchTag = string.Format("{0}{1}", SearchTag, c).ToLower();
			KvpTag = ListTag.FirstOrDefault(entry => string.Compare(entry.Key, SearchTag) >= 0);

			if (SongData.DictSong.ContainsKey(KvpTag.Value) &&
				KvpTag.Key.Length >= SearchTag.Length &&
				KvpTag.Key.Substring(0, SearchTag.Length) == SearchTag) {

				return KvpTag.Value;
			}
			return -1;
		}

		public static void ClearSearchTag() { SearchTag = ""; }
		public static string ShowSearchTag() { return SearchTag; }

		static SortedList<string, int> ListTag = null;
		public static void RefreshTagDB() {
			ListTag = new SortedList<string, int>();
			string str = "";
			foreach (KeyValuePair<int, SongData> kvp in SongData.DictSong) {
				str = kvp.Value.Title.ToLower();
				if (ListTag.ContainsKey(str)) { continue; }
				ListTag.Add(str, kvp.Value.ID);
			}
		}

		public static void DeleteFromTree(int id) {
			foreach (KeyValuePair<string, int> kvp in ListTag) {
				if (kvp.Value == id) {
					ListTag.Remove(kvp.Key);
					break;
				}
			}
		}
	}

	public struct TreeNode {
		public char Value;
		public int ParentIndex, SavedPoint, MinValueID, NextCount;

		public List<int> ListID;
		public Dictionary<char, int> DictNext;
	}
}
