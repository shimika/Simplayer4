using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		private List<KeyValuePair<string, int>> listSortTag = new List<KeyValuePair<string, int>>();
		private int CompareByKey(KeyValuePair<string, int> a, KeyValuePair<string, int> b) { return a.Key.CompareTo(b.Key); }

		public void SortList() {
			ListSong = SongData.DictSong.Values.ToList().OrderBy(x => x.SortTag).ThenBy(x => x.ID).ToList();

			buttonIndexerSort.Visibility = Visibility.Collapsed;
			gridIndexer.Visibility = Visibility.Visible;

			Pref.isSorted = true;

			for (int i = 0; i < ListSong.Count; i++) {
				stackList.Children.Remove(SongData.DictSong[ListSong[i].ID].GridBase);
				stackList.Children.Add(SongData.DictSong[ListSong[i].ID].GridBase);

				SongData.DictSong[ListSong[i].ID].Position = i;
			}
		}

		private void AddToLibrary(List<SongData> listAdd, bool isNew = true) {
			// Get List<SongData> (contains: Title, DurationString, FilePath)
			// Construct new SongData (contains: ID, SortTag, HeadIndex, ...)

			foreach (SongData sData in listAdd) {
				sData.ID = SongData.IDCount;

				int nHeaderIndex = GetIndexerHeaderFrom(sData.Title);
				sData.SortTag = string.Format("{0:D4}{1}", nHeaderIndex, sData.Title);
				sData.HeadIndex = IndexUnique.IndexOf(IndexValue[nHeaderIndex]);
				sData.Position = SongData.DictSong.Count;

				SongData.DictSong.Add(SongData.IDCount, sData);
				SongData.IDCount++;

				Grid grid = GetListItemButton(sData, isNew);
				SongData.DictSong[sData.ID].GridBase = grid;
				stackList.Children.Add(grid);

				((Button)grid.Children[3]).Click += SongListItem_Click;
				((Button)grid.Children[3]).MouseDoubleClick += SongListItem_DoubleClick;

			}

			if (isNew) {
				ShowMessage(string.Format("{0}개의 음악이 추가되었습니다.", listAdd.Count), 2);
			}

			// Push value to tree
			RefreshSortedPosition();
			foreach (SongData sData in listAdd) {
				TitleTree.AddToTree(sData.ID);
			}

			if (Pref.isAutoSort) {
				SortList();

				int firstIndex = (from item in listAdd select SongData.DictSong[item.ID].Position).Min();
				ScrollingList(firstIndex, 0);

			} else if (listAdd.Count > 0) {
				ListSong = SongData.DictSong.Values.ToList().OrderBy(x => x.Position).ThenBy(x => x.ID).ToList();
				RefreshAbsolutePositionByList();

				if (isNew || !Pref.isSorted) {
					gridIndexer.Visibility = Visibility.Collapsed;
					buttonIndexerSort.Visibility = Visibility.Visible;

					Pref.isSorted = false;
				}

				if (isNew) {
					ScrollingList(stackList.Children.Count - 1, -1);
				}
			}

			ShuffleList();
			SaveSongList();
			SavePreference();
		}

		private void RefreshAbsolutePositionByList() {
			for (int i = 0; i < ListSong.Count; i++) {
				ListSong[i].Position = i;
				SongData.DictSong[ListSong[i].ID].Position = i;
			}
		}
		private void RefreshAbsolutePositionByStackPanel() {
			int nIndex = 0;
			for (int i = 0; i < stackList.Children.Count; i++) {
				Grid grid = stackList.Children[i] as Grid;
				nIndex = (int)grid.Tag;
				SongData.DictSong[nIndex].Position = i;
			}
			ListSong = SongData.DictSong.Values.ToList().OrderBy(x => x.Position).ThenBy(x => x.ID).ToList();
		}
		private void RefreshSortedPosition() {
			List<SongData> listSorted = SongData.DictSong.Values.ToList().OrderBy(x => x.SortTag).ThenBy(x => x.ID).ToList();
			for (int i = 0; i < listSorted.Count; i++) {
				SongData.DictSong[listSorted[i].ID].SortPosition = i;
			}
			RefreshIndexer();
		}

		public static int[] PositionArray, ShuffleArray;
		public static void ShuffleList() {
			PositionArray = new int[SongData.DictSong.Count];
			ShuffleArray = new int[SongData.DictSong.Count];

			foreach (KeyValuePair<int, SongData> sData in SongData.DictSong) {
				PositionArray[sData.Value.Position] = sData.Key;
			}
			for (int i = 0; i < SongData.DictSong.Count; i++) {
				ShuffleArray[i] = i;
			}

			Random random = new Random();
			for (int k = 0; k < 3; k++) {
				for (int i = SongData.DictSong.Count - 1; i >= 0; i--) {
					int j = random.Next(i);
					int t = ShuffleArray[i]; ShuffleArray[i] = ShuffleArray[j]; ShuffleArray[j] = t;
				}
			}
		}
	}
}
