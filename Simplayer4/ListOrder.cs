using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simplayer4 {
	public class ListOrder {
		public static MainWindow winMain;

		static List<KeyValuePair<string, int>> listSortTag = new List<KeyValuePair<string, int>>();
		static int Compare1(KeyValuePair<string, int> a, KeyValuePair<string, int> b) { return a.Key.CompareTo(b.Key); }

		public static void ListSort() {
			winMain.buttonIndexerSort.Visibility = Visibility.Collapsed;
			winMain.gridIndexer.Visibility = Visibility.Visible;

			Pref.isSorted = true;
			PlayClass.RefreshSongPosition();

			listSortTag.Clear();
			foreach (KeyValuePair<int, SongData> kData in SongData.DictSong) {
				listSortTag.Add(new KeyValuePair<string, int>(kData.Value.strSortTag, kData.Key));
			}
			listSortTag.Sort(Compare1);
			//winMain.textTemp.Text = listSortTag[listSortTag.Count - 1].Key;

			for (int i = 0; i < listSortTag.Count; i++) {
				winMain.stackList.Children.Remove(SongData.DictSong[listSortTag[i].Value].gBase);
				winMain.stackList.Children.Add(SongData.DictSong[listSortTag[i].Value].gBase);
			}

			RefreshIndexer();
			FileIO.SavePreference();
		}

		public static int[] nIndexerPosition = new int[52];
		public static void RefreshIndexer() {
			if (!Pref.isSorted) { return; }
			for (int i = 0; i < nIndexerPosition.Length; i++) { nIndexerPosition[i] = -1; }

			PlayClass.RefreshSongPosition();

			foreach (KeyValuePair<int, SongData> kData in SongData.DictSong) {
				SongData sData = kData.Value;
				if (nIndexerPosition[sData.nHeadIndex] == -1) {
					nIndexerPosition[sData.nHeadIndex] = sData.nPosition;
				} else {
					nIndexerPosition[sData.nHeadIndex] = Math.Min(nIndexerPosition[sData.nHeadIndex], sData.nPosition);
				}
			}

			bool flag = false;
			for (int i = 41; i <= 50; i++) {
				if (nIndexerPosition[i] >= 0) { flag = true; break; }
			}

			for (int i = 0; i < nIndexerPosition.Length; i++) {
				if (nIndexerPosition[i] < 0) {
					(winMain.gridIndexer.Children[i] as Button).Background = Brushes.LightGray;
					(winMain.gridIndexer.Children[i] as Button).IsEnabled = false;
				} else {
					(winMain.gridIndexer.Children[i] as Button).SetResourceReference(Button.BackgroundProperty, "sColor");
					(winMain.gridIndexer.Children[i] as Button).IsEnabled = true;
				}

				(winMain.gridIndexer.Children[i] as Button).Visibility = flag || i > 50 || i < 41 ? Visibility.Visible : Visibility.Collapsed;
			}

			Grid.SetRow(winMain.gridIndexer.Children[51] as Button, flag ? 7 : 5);
		}
	}
}
