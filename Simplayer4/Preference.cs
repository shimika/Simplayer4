using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		private void SetPreferenceEvent() {
			buttonClickOne.Click += buttonClickOne_Click;
			buttonClickDouble.Click += buttonClickDouble_Click;

			buttonNotifyOn.Click += buttonNotifyOn_Click;
			buttonNotifyOff.Click += buttonNotifyOff_Click;

			buttonAutoSortOn.Click += buttonAutoSortOn_Click;
			buttonAutoSortOff.Click += buttonAutoSortOff_Click;

			buttonTray.Click += buttonTray_Click;
			buttonTaskbar.Click += buttonTaskbar_Click;

			buttonHotkeyOn.Click += buttonHotkeyOn_Click;
			buttonHotkeyOff.Click += buttonHotkeyOff_Click;

			buttonTopmostOn.Click += buttonTopmostOn_Click;
			buttonTopmostOff.Click += buttonTopmostOff_Click;

			buttonLyricsLeft.Click += buttonLyricsLeft_Click;
			buttonLyricsRight.Click += buttonLyricsRight_Click;

			// Setting line
			buttonLyricsOff.Click += buttonLyricsOff_Click;
			buttonLyricsOn.Click += buttonLyricsOn_Click;

			buttonRandom.Click += buttonRandom_Click;
			buttonLinear.Click += buttonLinear_Click;

			buttonRepeat.Click += buttonRepeat_Click;
			buttonPlayAll.Click += buttonPlayAll_Click;

			textShortcutScript.ToolTip = "글로벌 단축키를 켤 지의 여부를 설정합니다.\n단축키는 기본적으로 Ctrl + Alt의 조합으로 만들 수 있습니다.\nCtrl + Alt + ← : 이전 곡\nCtrl + Alt + → : 다음 곡\nCtrl + Alt + ↑ : 정지\nCtrl + Alt + ↓ : 재생/일시정지\nCtrl + Alt + D : 싱크 가사 켜기/끄기\nCtrl + Alt + , : 싱크 가사 0.2초 앞으로\nCtrl + Alt + . : 싱크 가사 0.2초 뒤로";

			// Color change control

			int nThemeCount = 0;
			foreach (Button button in gridTheme.Children) {
				nThemeCount++;

				// 6개만 프리셋을 설정
				if (nThemeCount == 7) { break; }

				button.Click += (o, e) => {
					((Button)gridTheme.Children[7]).Background = Brushes.White;

					Pref.ThemeCode = Convert.ToInt32((string)((Button)o).Tag);
					SolidColorBrush sc = (SolidColorBrush)((Button)o).Background;
					ChangeThemeColor(sc.Color);
				};
			}

			// 앨범아트 이미지 컬러
			((Button)gridTheme.Children[6]).Click += (o, e) => {
				Pref.ThemeCode = 6;

				if (Pref.isPlaying == 0) {
					ChangeThemeColor(Colors.Black);
				} else {
					// 현재 앨범아트의 평균 색상을 가져와서 변경
					Color c = TagLibrary.CalculateAverageColor((BitmapSource)imageAlbumart.Source);
					ChangeThemeColor(c);
				}
			};

			// 커스텀 컬러
			((Button)gridTheme.Children[7]).Click += (o, e) => {
				System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog() {
					AllowFullOpen = true,
				};
				if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
					Pref.ThemeCode = 7;
					Pref.ThemeColor = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);

					((Button)gridTheme.Children[7]).Background = new SolidColorBrush(Pref.ThemeColor);

					ChangeThemeColor(Pref.ThemeColor);
				}
			};
		}
		private void SetPreferenceToControl() {
			if (Pref.ThemeCode < 6) {
				ChangeThemeColor(((SolidColorBrush)((Button)gridTheme.Children[Pref.ThemeCode]).Background).Color);
			} else if (Pref.ThemeCode == 6) {
				ChangeThemeColor(Colors.Black);
			} else if (Pref.ThemeCode == 7) {
				((Button)gridTheme.Children[7]).Background = new SolidColorBrush(Pref.ThemeColor);
				ChangeThemeColor(Pref.ThemeColor);
			}

			buttonLyricsOff.Visibility = !Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
			buttonLyricsOn.Visibility = Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
			LyrMenuItem.Checked = Pref.isLyricsVisible;

			buttonLinear.Visibility = Pref.RandomSeed == 1 ? Visibility.Visible : Visibility.Collapsed;
			buttonRandom.Visibility = Pref.RandomSeed == 2 ? Visibility.Visible : Visibility.Collapsed;

			buttonPlayAll.Visibility = Pref.PlayingLoopSeed == 1 ? Visibility.Visible : Visibility.Collapsed;
			buttonRepeat.Visibility = Pref.PlayingLoopSeed == 0 ? Visibility.Visible : Visibility.Collapsed;

			buttonHideList.Visibility = Pref.isListVisible ? Visibility.Visible : Visibility.Collapsed;
			buttonShowList.Visibility = !Pref.isListVisible ? Visibility.Visible : Visibility.Collapsed;
			if (!Pref.isListVisible) { this.Height = this.MinHeight = this.MaxHeight = 192; }

			OptionColorBinder(buttonClickOne.Content, Pref.isOneClickPlaying);
			OptionColorBinder(buttonClickDouble.Content, !Pref.isOneClickPlaying);

			OptionColorBinder(buttonNotifyOn.Content, Pref.isNofifyOn);
			OptionColorBinder(buttonNotifyOff.Content, !Pref.isNofifyOn);

			OptionColorBinder(buttonAutoSortOn.Content, Pref.isAutoSort);
			OptionColorBinder(buttonAutoSortOff.Content, !Pref.isAutoSort);

			OptionColorBinder(buttonTray.Content, Pref.isTray);
			OptionColorBinder(buttonTaskbar.Content, !Pref.isTray);
			if (!Pref.isTray) { this.ShowInTaskbar = true; }

			OptionColorBinder(buttonHotkeyOn.Content, Pref.isHotkeyOn);
			OptionColorBinder(buttonHotkeyOff.Content, !Pref.isHotkeyOn);

			OptionColorBinder(buttonTopmostOn.Content, Pref.isTopMost);
			OptionColorBinder(buttonTopmostOff.Content, !Pref.isTopMost);
			this.Topmost = Pref.isTopMost;

			OptionColorBinder(buttonLyricsLeft.Content, !Pref.isLyricsRight);
			OptionColorBinder(buttonLyricsRight.Content, Pref.isLyricsRight);

			gridIndexer.Visibility = Pref.isSorted ? Visibility.Visible : Visibility.Collapsed;
			buttonIndexerSort.Visibility = !Pref.isSorted ? Visibility.Visible : Visibility.Collapsed;

			rectVolume.Width = Pref.Volume;
			mp.Volume = Pref.Volume / 50;
		}

		private void buttonClickOne_Click(object sender, RoutedEventArgs e) {
			if (Pref.isOneClickPlaying) { return; }
			Pref.isOneClickPlaying = true;
			((TextBlock)buttonClickOne.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)buttonClickDouble.Content).Foreground = Brushes.LightGray;
			SavePreference();
		}
		private void buttonClickDouble_Click(object sender, RoutedEventArgs e) {
			if (!Pref.isOneClickPlaying) { return; }
			Pref.isOneClickPlaying = false;
			((TextBlock)buttonClickOne.Content).Foreground = Brushes.LightGray;
			((TextBlock)buttonClickDouble.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			SavePreference();
		}

		private void buttonNotifyOn_Click(object sender, RoutedEventArgs e) {
			if (Pref.isNofifyOn) { return; }
			Pref.isNofifyOn = true;
			((TextBlock)buttonNotifyOn.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)buttonNotifyOff.Content).Foreground = Brushes.LightGray;
			SavePreference();
		}
		private void buttonNotifyOff_Click(object sender, RoutedEventArgs e) {
			if (!Pref.isNofifyOn) { return; }
			Pref.isNofifyOn = false;
			((TextBlock)buttonNotifyOn.Content).Foreground = Brushes.LightGray;
			((TextBlock)buttonNotifyOff.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			SavePreference();
		}

		private void buttonAutoSortOn_Click(object sender, RoutedEventArgs e) {
			if (Pref.isAutoSort) { return; }

			Pref.isAutoSort = true;
			((TextBlock)buttonAutoSortOn.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)buttonAutoSortOff.Content).Foreground = Brushes.LightGray;

			SortList();
			ShuffleList();
			SaveSongList();
			SavePreference();
		}
		private void buttonAutoSortOff_Click(object sender, RoutedEventArgs e) {
			if (!Pref.isAutoSort) { return; }

			Pref.isAutoSort = false;
			((TextBlock)buttonAutoSortOn.Content).Foreground = Brushes.LightGray;
			((TextBlock)buttonAutoSortOff.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");

			SavePreference();
		}

		private void buttonTray_Click(object sender, RoutedEventArgs e) {
			if (Pref.isTray) { return; }
			Pref.isTray = true;
			ShowInTaskbar = false;

			((TextBlock)buttonTray.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)buttonTaskbar.Content).Foreground = Brushes.LightGray;
			SavePreference();

			SortList();
		}
		private void buttonTaskbar_Click(object sender, RoutedEventArgs e) {
			if (!Pref.isTray) { return; }
			Pref.isTray = false;
			ShowInTaskbar = true;

			((TextBlock)buttonTray.Content).Foreground = Brushes.LightGray;
			((TextBlock)buttonTaskbar.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			SavePreference();
		}

		private void buttonHotkeyOn_Click(object sender, RoutedEventArgs e) {
			if (Pref.isHotkeyOn) { return; }
			Pref.isHotkeyOn = true;

			((TextBlock)buttonHotkeyOn.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)buttonHotkeyOff.Content).Foreground = Brushes.LightGray;
			SavePreference();

			SortList();
		}
		private void buttonHotkeyOff_Click(object sender, RoutedEventArgs e) {
			if (!Pref.isHotkeyOn) { return; }
			Pref.isHotkeyOn = false;

			((TextBlock)buttonHotkeyOn.Content).Foreground = Brushes.LightGray;
			((TextBlock)buttonHotkeyOff.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			SavePreference();
		}

		private void buttonTopmostOn_Click(object sender, RoutedEventArgs e) {
			if (Pref.isTopMost) { return; }
			Pref.isTopMost = true;
			Topmost = true;

			((TextBlock)buttonTopmostOn.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)buttonTopmostOff.Content).Foreground = Brushes.LightGray;
			SavePreference();
		}
		private void buttonTopmostOff_Click(object sender, RoutedEventArgs e) {
			if (!Pref.isTopMost) { return; }
			Pref.isTopMost = false;
			Topmost = false;

			((TextBlock)buttonTopmostOn.Content).Foreground = Brushes.LightGray;
			((TextBlock)buttonTopmostOff.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			SavePreference();
		}

		private void buttonLyricsLeft_Click(object sender, RoutedEventArgs e) {
			if (!Pref.isLyricsRight) { return; }
			Pref.isLyricsRight = false;

			((TextBlock)buttonLyricsLeft.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)buttonLyricsRight.Content).Foreground = Brushes.LightGray;
			SavePreference();
		}
		private void buttonLyricsRight_Click(object sender, RoutedEventArgs e) {
			if (Pref.isLyricsRight) { return; }
			Pref.isLyricsRight = true;

			((TextBlock)buttonLyricsLeft.Content).Foreground = Brushes.LightGray;
			((TextBlock)buttonLyricsRight.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			SavePreference();
		}

		private void buttonLyricsOff_Click(object sender, RoutedEventArgs e) {
			Pref.isLyricsVisible = true;
			LyrMenuItem.Checked = Pref.isLyricsVisible;

			buttonLyricsOn.Visibility = Visibility.Visible;
			buttonLyricsOff.Visibility = Visibility.Collapsed;

			LyricsWindow.ToggleLyrics(Pref.isLyricsVisible);
			SavePreference();
		}
		private void buttonLyricsOn_Click(object sender, RoutedEventArgs e) {
			Pref.isLyricsVisible = false;
			LyrMenuItem.Checked = Pref.isLyricsVisible;

			buttonLyricsOn.Visibility = Visibility.Collapsed;
			buttonLyricsOff.Visibility = Visibility.Visible;
			LyricsWindow.ToggleLyrics(Pref.isLyricsVisible);
			SavePreference();
		}

		private void buttonRandom_Click(object sender, RoutedEventArgs e) {
			Pref.RandomSeed = 1;
			buttonRandom.Visibility = Visibility.Collapsed;
			buttonLinear.Visibility = Visibility.Visible;
			SavePreference(); 
		}
		private void buttonLinear_Click(object sender, RoutedEventArgs e) {

			Pref.RandomSeed = 2;
			buttonRandom.Visibility = Visibility.Visible;
			buttonLinear.Visibility = Visibility.Collapsed;
			SavePreference(); 
		}

		private void buttonRepeat_Click(object sender, RoutedEventArgs e) {
			Pref.PlayingLoopSeed = 1;
			buttonRepeat.Visibility = Visibility.Collapsed;
			buttonPlayAll.Visibility = Visibility.Visible;
			SavePreference();
		}
		private void buttonPlayAll_Click(object sender, RoutedEventArgs e) {
			Pref.PlayingLoopSeed = 0;
			buttonRepeat.Visibility = Visibility.Visible;
			buttonPlayAll.Visibility = Visibility.Collapsed;
			SavePreference();
		}
	}
}
