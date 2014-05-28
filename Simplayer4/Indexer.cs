using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		private string IndexHeader = "1RSEFAQTDWCZXVGABCDEFGHIJKLMNOPQRSTUVWXYZAKSTNHMYRW";
		private string IndexerFormattedHeader = "1ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎ_ABCDEFGHIJKLMNOPQRSTUVWXYZ______あかさたなはまやらわ#";
		private string IndexCaption = "0123456789ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZぁァあアぃィいイぅゥうウぇェえエぉォおオかカがガきキぎギくクぐグけケげゲこコごゴさサざザしシじジすスずズせセぜゼそソぞゾたタだダちチぢヂっッつツづヅてテでデとトどドなナにニぬヌねネのノはハばバぱパひヒびビぴピふフぶブぷプへヘべベぺペほホぼボぽポまマみミむムめメもモゃャやヤゅュゆユょョよヨらラりリるルれレろロゎヮわワをヲんンヴ―#";
		private string IndexValue = "1111111111ㄱㄱㄴㄷㄷㄹㅁㅂㅂㅅㅅㅇㅈㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZああああああああああああああああああああかかかかかかかかかかかかかかかかかかかかささささささささささささささささささささたたたたたたたたたたたたたたたたたたたたたたななななななななななははははははははははははははははははははははははははははははままままままままままややややややややややややららららららららららわわわわわわわわわわ#";
		private string IndexUnique = "1ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZあかさたなはまやらわ#";

		private void IndexerPreset() {
			int nCounter = 0;
			for (int i = 0; i < IndexerFormattedHeader.Length; i++) {
				if (IndexerFormattedHeader[i] == '_') { continue; }
				Button button = new Button() {
					Width = 36, Height = 36, Style = FindResource("FlatButton") as Style,
					HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
					Tag = nCounter++,
				};
				button.SetResourceReference(Button.BackgroundProperty, "sColor");

				TextBlock txt = new TextBlock() {
					Foreground = Brushes.White, Margin = new Thickness(4, 2, 0, 0), FontSize = 14,
					Text = IndexerFormattedHeader[i].ToString(),
					TextAlignment = TextAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
				};

				button.Content = txt;
				button.Click += (o, e) => {
					int nIndex = (int)((Button)o).Tag;
					LaunchIndexerKey(nIndex);
				};

				Grid.SetRow(button, i / 8);
				Grid.SetColumn(button, i % 8);
				gridIndexer.Children.Add(button);
			}
		}

		private void LaunchIndexerKey(int nIndex) {
			if (nIndex < 0) { return; }
			//textTemp.Text = nIndexerPosition[nIndex].ToString();
			gridIndexerRoot.Visibility = Visibility.Collapsed;

			if (!Pref.isSorted) { return; }
			if (IndexerPosition[nIndex] < 0) { return; }

			ChangeSelection((int)((Grid)stackList.Children[IndexerPosition[nIndex]]).Tag);
			ScrollingList(IndexerPosition[nIndex], 0);
		}

		private int GetIndexerHeaderFrom(string songTitle) {
			char cHead = HangulDevide(songTitle.ToUpper())[0];
			int idx = IndexCaption.IndexOf(cHead);
			if (idx < 0) { idx += IndexCaption.Length; }

			return idx;
		}

		public int[] IndexerPosition = new int[52];
		public void RefreshIndexer() {
			IndexerPosition = Array.ConvertAll<int, int>(IndexerPosition, x => x = -1);

			foreach (KeyValuePair<int, SongData> kData in SongData.DictSong) {
				SongData sData = kData.Value;
				if (IndexerPosition[sData.HeadIndex] == -1) {
					IndexerPosition[sData.HeadIndex] = sData.SortPosition;
				} else {
					IndexerPosition[sData.HeadIndex] = Math.Min(IndexerPosition[sData.HeadIndex], sData.SortPosition);
				}
			}

			// Japan character

			bool flag = false;
			for (int i = 41; i <= 50; i++) {
				if (IndexerPosition[i] >= 0) {
					flag = true; 
					break;
				}
			}

			for (int i = 0; i < IndexerPosition.Length; i++) {
				if (IndexerPosition[i] < 0) {
					(gridIndexer.Children[i] as Button).Background = Brushes.LightGray;
					(gridIndexer.Children[i] as Button).IsEnabled = false;
				} else {
					(gridIndexer.Children[i] as Button).SetResourceReference(Button.BackgroundProperty, "sColor");
					(gridIndexer.Children[i] as Button).IsEnabled = true;
				}

				(gridIndexer.Children[i] as Button).Visibility 
					= flag || i > 50 || i < 41 ? Visibility.Visible : Visibility.Collapsed;
			}

			// Button '#'

			Grid.SetRow(gridIndexer.Children[51] as Button, flag ? 7 : 5);
		}
	}
}
