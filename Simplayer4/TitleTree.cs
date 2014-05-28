using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Simplayer4 {
	public class TitleTree {
		static int LastPushPosition;
		public static List<TreeNode> ListTree = new List<TreeNode>();
		public static int LastPointer = 0;

		public static void AddToTree(int nID) {
			int it = 0, mIndex = 0;
			TreeNode tn;

			if (ListTree.Count == 0) {
				ListTree.Add(new TreeNode() {
					DictNext = new Dictionary<char, int>(),
				});
				LastPushPosition = 1;
			}

			string title = ReformTitle(SongData.DictSong[nID].Title);
			if (title.Length == 0) { return; }

			foreach (char c in title) {
				if (ListTree[it].DictNext.ContainsKey(c)) {
					it = ListTree[it].DictNext[c];
				} else {
					if (LastPushPosition == ListTree.Count) {
						ListTree.Add(new TreeNode() {
							Value = c, ParentIndex = it,
							DictNext = new Dictionary<char, int>(),
							NextCount = 0, MinValueID = -1,
						});

						ListTree[it].DictNext.Add(c, ListTree.Count - 1);

						it = ListTree.Count - 1;
						LastPushPosition = ListTree.Count;
					} else {
						ListTree[LastPushPosition] = new TreeNode() {
							Value = c, ParentIndex = it,
							DictNext = new Dictionary<char, int>(),
							NextCount = 0, MinValueID = -1,
						};

						ListTree[it].DictNext.Add(c, LastPushPosition);

						it = LastPushPosition;
						LastPushPosition = ListTree[LastPushPosition].SavedPoint;
					}
				}
			}

			tn = ListTree[it];
			if (ListTree[it].ListID == null) {
				tn.ListID = new List<int>();
			}
			tn.ListID.Add(nID);
			ListTree[it] = tn;

			mIndex = FindMin(it);

			for (; ; ) {
				tn = ListTree[it];

				tn.NextCount++;
				if (tn.MinValueID < 0) {
					tn.MinValueID = mIndex;
				} else {
					if (SongData.DictSong[tn.MinValueID].SortPosition > SongData.DictSong[mIndex].SortPosition) {
						tn.MinValueID = mIndex;
					} else {
						mIndex = tn.MinValueID;
					}
				}

				ListTree[it] = tn;

				if (it == 0) { break; }

				it = ListTree[it].ParentIndex;
			}
		}

		public static void DeleteFromTree(int nID) {
			string title = ReformTitle(SongData.DictSong[nID].Title);
			if (title.Length == 0) { return; }

			int it = 0;
			foreach (char c in title) {
				if (ListTree[it].DictNext.ContainsKey(c)) {
					it = ListTree[it].DictNext[c];
				} else {
					return;
				}
			}

			TreeNode tn = ListTree[it];
			try {
				tn.ListID.Remove(nID);
			} catch { return; }

			int mIndex = FindMin(it), savedIndex = ListTree[it].MinValueID;

			ListTree[it] = tn;

			for (; ; ) {
				tn = ListTree[it];

				tn.NextCount--;
				if (tn.NextCount == 0) {
					tn.SavedPoint = LastPushPosition;

					TreeNode tNode = ListTree[tn.ParentIndex];
					tNode.DictNext.Remove(tn.Value);
					ListTree[tn.ParentIndex] = tNode;
				} else {
					if (tn.MinValueID == nID) {
						tn.MinValueID = mIndex = FindMin(it);
					} else {
						mIndex = tn.MinValueID;
					}
				}

				ListTree[it] = tn;

				if (it == 0) { break; }
				it = ListTree[it].ParentIndex;
			}
		}

		private static int FindMin(int nodeX) {
			int mValue = 999999999, mIndex = 0;

			if (ListTree[nodeX].ListID != null) {
				foreach (int id in ListTree[nodeX].ListID) {
					if (mValue > SongData.DictSong[id].SortPosition) {
						mValue = SongData.DictSong[id].SortPosition;
						mIndex = id;
					}
				}
			}
			foreach (KeyValuePair<char, int> node in ListTree[nodeX].DictNext) {
				if (mValue > SongData.DictSong[ListTree[node.Value].MinValueID].SortPosition) {
					mValue = SongData.DictSong[ListTree[node.Value].MinValueID].SortPosition;
					mIndex = ListTree[node.Value].MinValueID;
				}
			}
			return mIndex;
		}

		public static string ReformTitle(string title) {
			title = title.ToUpper();
			string raw = "";

			foreach (char c in title) {
				if (c == ' ') { continue; }
				if (!Char.IsUpper(c)) { break; }
				raw += c;
			}

			return raw;
		}

		public static int GetPositionByHeader(char c) {
			if (ListTree[LastPointer].DictNext.ContainsKey(c)) {
				LastPointer = ListTree[LastPointer].DictNext[c];
				return ListTree[LastPointer].MinValueID;
			}
			return -1;
		}
	}

	public struct TreeNode {
		public char Value;
		public int ParentIndex, SavedPoint, MinValueID, NextCount;

		public List<int> ListID;
		public Dictionary<char, int> DictNext;
	}
}
