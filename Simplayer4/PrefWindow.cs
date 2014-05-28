using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Simplayer4 {
	public class PrefWindow {
		public static MainWindow winMain;
		public static void PrefWindowPreset() {			// Pref Window
			winMain.buttonClickOne.Click += (o, e) => {
				if (Pref.isOneClickPlaying) { return; }
				Pref.isOneClickPlaying = true;
				((TextBlock)winMain.buttonClickOne.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				((TextBlock)winMain.buttonClickDouble.Content).Foreground = Brushes.LightGray;
				FileIO.SavePreference();
			};
			winMain.buttonClickDouble.Click += (o, e) => {
				if (!Pref.isOneClickPlaying) { return; }
				Pref.isOneClickPlaying = false;
				((TextBlock)winMain.buttonClickOne.Content).Foreground = Brushes.LightGray;
				((TextBlock)winMain.buttonClickDouble.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				FileIO.SavePreference();
			};

			winMain.buttonNotifyOn.Click += (o, e) => {
				if (Pref.isNofifyOn) { return; }
				Pref.isNofifyOn = true;
				((TextBlock)winMain.buttonNotifyOn.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				((TextBlock)winMain.buttonNotifyOff.Content).Foreground = Brushes.LightGray;
				FileIO.SavePreference();
			};
			winMain.buttonNotifyOff.Click += (o, e) => {
				if (!Pref.isNofifyOn) { return; }
				Pref.isNofifyOn = false;
				((TextBlock)winMain.buttonNotifyOn.Content).Foreground = Brushes.LightGray;
				((TextBlock)winMain.buttonNotifyOff.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				FileIO.SavePreference();
			};

			winMain.buttonAutoSortOn.Click += (o, e) => {
				if (Pref.isAutoSort) { return; }
				Pref.isAutoSort = true;
				((TextBlock)winMain.buttonAutoSortOn.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				((TextBlock)winMain.buttonAutoSortOff.Content).Foreground = Brushes.LightGray;
				FileIO.SavePreference();

				ListOrder.ListSort();
			};
			winMain.buttonAutoSortOff.Click += (o, e) => {
				if (!Pref.isAutoSort) { return; }
				Pref.isAutoSort = false;
				((TextBlock)winMain.buttonAutoSortOn.Content).Foreground = Brushes.LightGray;
				((TextBlock)winMain.buttonAutoSortOff.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				FileIO.SavePreference();
			};

			winMain.buttonTray.Click += (o, e) => {
				if (Pref.isTray) { return; }
				Pref.isTray = true;
				winMain.ShowInTaskbar = false;

				((TextBlock)winMain.buttonTray.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				((TextBlock)winMain.buttonTaskbar.Content).Foreground = Brushes.LightGray;
				FileIO.SavePreference();

				ListOrder.ListSort();
			};
			winMain.buttonTaskbar.Click += (o, e) => {
				if (!Pref.isTray) { return; }
				Pref.isTray = false;
				winMain.ShowInTaskbar = true;

				((TextBlock)winMain.buttonTray.Content).Foreground = Brushes.LightGray;
				((TextBlock)winMain.buttonTaskbar.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				FileIO.SavePreference();
			};

			winMain.buttonHotkeyOn.Click += (o, e) => {
				if (Pref.isHotkeyOn) { return; }
				Pref.isHotkeyOn = true;

				((TextBlock)winMain.buttonHotkeyOn.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				((TextBlock)winMain.buttonHotkeyOff.Content).Foreground = Brushes.LightGray;
				FileIO.SavePreference();

				ListOrder.ListSort();
			};
			winMain.buttonHotkeyOff.Click += (o, e) => {
				if (!Pref.isHotkeyOn) { return; }
				Pref.isHotkeyOn = false;

				((TextBlock)winMain.buttonHotkeyOn.Content).Foreground = Brushes.LightGray;
				((TextBlock)winMain.buttonHotkeyOff.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				FileIO.SavePreference();
			};
			winMain.textShortcutScript.ToolTip = "글로벌 단축키를 켤 지의 여부를 설정합니다.\n단축키는 기본적으로 Ctrl + Alt의 조합으로 만들 수 있습니다.\nCtrl + Alt + ← : 이전 곡\nCtrl + Alt + → : 다음 곡\nCtrl + Alt + ↑ : 정지\nCtrl + Alt + ↓ : 재생/일시정지\nCtrl + Alt + D : 싱크 가사 켜기/끄기\nCtrl + Alt + , : 싱크 가사 0.2초 앞으로\nCtrl + Alt + . : 싱크 가사 0.2초 뒤로";

			winMain.buttonTopmostOn.Click += (o, e) => {
				if (Pref.isTopMost) { return; }
				Pref.isTopMost = true;
				winMain.Topmost = true;

				((TextBlock)winMain.buttonTopmostOn.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				((TextBlock)winMain.buttonTopmostOff.Content).Foreground = Brushes.LightGray;
				FileIO.SavePreference();
			};
			winMain.buttonTopmostOff.Click += (o, e) => {
				if (!Pref.isTopMost) { return; }
				Pref.isTopMost = false;
				winMain.Topmost = false;

				((TextBlock)winMain.buttonTopmostOn.Content).Foreground = Brushes.LightGray;
				((TextBlock)winMain.buttonTopmostOff.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				FileIO.SavePreference();
			};

			winMain.buttonLyricsLeft.Click += (o, e) => {
				if (!Pref.isLyricsRight) { return; }
				Pref.isLyricsRight = false;

				((TextBlock)winMain.buttonLyricsLeft.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				((TextBlock)winMain.buttonLyricsRight.Content).Foreground = Brushes.LightGray;
				FileIO.SavePreference();
			};

			winMain.buttonLyricsRight.Click += (o, e) => {
				if (Pref.isLyricsRight) { return; }
				Pref.isLyricsRight = true;

				((TextBlock)winMain.buttonLyricsLeft.Content).Foreground = Brushes.LightGray;
				((TextBlock)winMain.buttonLyricsRight.Content).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				FileIO.SavePreference();
			};

			// Setting line
			winMain.buttonLyricsOff.Click += (o, e) => {
				Pref.isLyricsVisible = true;
				winMain.LyrWindow.Checked = Pref.isLyricsVisible;

				winMain.buttonLyricsOn.Visibility = Visibility.Visible;
				winMain.buttonLyricsOff.Visibility = Visibility.Collapsed;

				winMain.LyricsWindow.ToggleLyrics(Pref.isLyricsVisible);
				FileIO.SavePreference();
			};
			winMain.buttonLyricsOn.Click += (o, e) => {
				Pref.isLyricsVisible = false;
				winMain.LyrWindow.Checked = Pref.isLyricsVisible;

				winMain.buttonLyricsOn.Visibility = Visibility.Collapsed;
				winMain.buttonLyricsOff.Visibility = Visibility.Visible;
				winMain.LyricsWindow.ToggleLyrics(Pref.isLyricsVisible);
				FileIO.SavePreference();
			};

			winMain.buttonRandom.Click += (o, e) => {
				Pref.nRandomSeed = 1;
				winMain.buttonRandom.Visibility = Visibility.Collapsed;
				winMain.buttonLinear.Visibility = Visibility.Visible;
				FileIO.SavePreference();
			};
			winMain.buttonLinear.Click += (o, e) => {
				Pref.nRandomSeed = 2;
				winMain.buttonRandom.Visibility = Visibility.Visible;
				winMain.buttonLinear.Visibility = Visibility.Collapsed;
				FileIO.SavePreference();
			};

			winMain.buttonRepeat.Click += (o, e) => {
				Pref.nPlayingLoopSeed = 1;
				winMain.buttonRepeat.Visibility = Visibility.Collapsed;
				winMain.buttonPlayAll.Visibility = Visibility.Visible;
				FileIO.SavePreference();
			};
			winMain.buttonPlayAll.Click += (o, e) => {
				Pref.nPlayingLoopSeed = 0;
				winMain.buttonRepeat.Visibility = Visibility.Visible;
				winMain.buttonPlayAll.Visibility = Visibility.Collapsed;
				FileIO.SavePreference();
			};
		}
	}
}
