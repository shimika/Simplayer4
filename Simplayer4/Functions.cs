using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		// Delete file
		bool isDeleteProcessing = false;
		public void FileDelete(int deleteID) {
			if (deleteID < 0 || isDeleteProcessing || !SongData.DictSong.ContainsKey(deleteID)) { return; }

			// Delete delay (100 ms)
			isDeleteProcessing = true;
			DispatcherTimer DeleteDelayTimer = new DispatcherTimer() {
				Interval = TimeSpan.FromMilliseconds(100), 
				IsEnabled = true,
			};
			DeleteDelayTimer.Tick += DeleteDelayTimer_Tick;

			try { (SongData.DictSong[deleteID].GridBase.Children[4] as Grid).Children.Clear(); } catch { }

			TitleTree.DeleteFromTree(deleteID);

			int deleteIndex = SongData.DictSong[deleteID].Position;
			stackList.Children.RemoveAt(deleteIndex);
			ListSong.RemoveAt(deleteIndex);
			SongData.DictSong.Remove(deleteID);

			RefreshAbsolutePositionByList();
			RefreshSortedPosition();

			ShuffleList();
			SaveSongList();
			SavePreference();

			if (deleteID == SongData.NowSelected && SongData.DictSong.Count > 0) {
				SongData.NowSelected = ListSong[Math.Min(SongData.DictSong.Count - 1, deleteIndex)].ID;
				ChangeSelection(SongData.NowSelected);
			}
		}
		private void DeleteDelayTimer_Tick(object sender, EventArgs e) {
			(sender as DispatcherTimer).Stop();
			isDeleteProcessing = false; 
		}

		// Activate window by preference
		private void ActivateWindow() {
			if (Pref.isTray) {
				this.WindowState = WindowState.Normal;
				Pref.isShowing = true;
				this.Topmost = false;
				this.Topmost = Pref.isTopMost;

				new AltTab().ShowAltTab(this);
				this.BeginAnimation(Window.OpacityProperty,
					new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)) {
						EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
					});
				this.Activate();
			} else {
				this.WindowState = WindowState.Normal;
			}
		}

		// Show error or info message
		public void ShowMessage(string message, double dTimeout) {
			Storyboard sb = new Storyboard();

			if (dTimeout > 0) {
				ThicknessAnimation ta1 = new ThicknessAnimation(new Thickness(0, -30, 0, 0), TimeSpan.FromMilliseconds(200)) {
					BeginTime = TimeSpan.FromMilliseconds(100),
					EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
				};
				textMessage.Text = message;
				Storyboard.SetTarget(ta1, gridMessage);
				Storyboard.SetTargetProperty(ta1, new PropertyPath(Grid.MarginProperty));
				sb.Children.Add(ta1);
			}

			ThicknessAnimation ta2 = new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(200)) {
				BeginTime = TimeSpan.FromSeconds(dTimeout),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
			};

			Storyboard.SetTarget(ta2, gridMessage);
			Storyboard.SetTargetProperty(ta2, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(ta2);
			sb.Begin(this);
		}

		// If app already executed
		public bool ProcessCommandLineArgs(IList<string> args) {
			ActivateWindow();
			if (!Pref.isListVisible) { ToggleList(); }

			if (args == null || args.Count == 0) { return true; }

			return true;
		}

		// Change main theme color
		public void ChangeThemeColor(Color color, bool isSave = true) {
			MainColor = color;
			grideffectShadow.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation(color, TimeSpan.FromMilliseconds(250)));
			gStop1.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation(color, TimeSpan.FromMilliseconds(250)));

			Application.Current.Resources["sColor"] = new SolidColorBrush(MainColor);
			Application.Current.Resources["cColor"] = Color.FromArgb(255, MainColor.R, MainColor.G, MainColor.B);
			sColor = MainBrush = FindResource("sColor") as SolidColorBrush;

			if (isSave) { SavePreference(); }
		}

		// Textblock color binding
		private void OptionColorBinder(object uie, bool b) {
			TextBlock txt = uie as TextBlock;
			if (b) {
				txt.SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			} else { txt.Foreground = Brushes.LightGray; }
		}

		// Change now selected item
		private void ChangeSelection(int tag) {
			if (SongData.NowSelected >= 0 && SongData.DictSong.ContainsKey(SongData.NowSelected)) {
				((TextBlock)SongData.DictSong[SongData.NowSelected].GridBase.Children[0]).FontWeight = FontWeights.Normal;
				((TextBlock)SongData.DictSong[SongData.NowSelected].GridBase.Children[0]).Foreground = Brushes.Black;
			}
			SongData.NowSelected = tag;

			((TextBlock)SongData.DictSong[SongData.NowSelected].GridBase.Children[0]).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)SongData.DictSong[SongData.NowSelected].GridBase.Children[0]).FontWeight = FontWeights.ExtraBold;
		}

		// Scroll animation to position
		private void ScrollingList(int itemIndex, int offset, double duration = 500) {
			double nowOffset = scrollList.VerticalOffset, newOffset = 0;
			double TowardOffset = -1;

			if (offset < 0) {
				newOffset = nowOffset;
				if (itemIndex * 40 < nowOffset) {
					newOffset = itemIndex * 40;
				}
				if ((itemIndex + 1) * 40 > nowOffset + gridListArea.ActualHeight - 40) {
					newOffset = nowOffset + (((itemIndex + 1) * 40) - (nowOffset + gridListArea.ActualHeight - 40));
				}
				if (nowOffset == newOffset) { return; }
			} else { newOffset = (itemIndex - offset) * 40; }
			double Duration = Math.Abs(newOffset - nowOffset);

			newOffset = Math.Min(newOffset, SongData.DictSong.Count * 40 - gridListArea.ActualHeight);
			Storyboard sb = new Storyboard();

			if (TowardOffset > 0) { nowOffset = TowardOffset; }
			DoubleAnimation da = new DoubleAnimation(nowOffset, newOffset, TimeSpan.FromMilliseconds(duration)) {
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
				BeginTime = TimeSpan.FromMilliseconds(0),
			};
			Storyboard.SetTarget(da, scrollList);
			Storyboard.SetTargetProperty(da, new PropertyPath(AniScrollViewer.CurrentVerticalOffsetProperty));

			sb.Children.Add(da);
			sb.Completed += delegate(object sender, EventArgs e) { TowardOffset = -1; };
			sb.Begin(this);

			TowardOffset = newOffset;
		}

		// Reset player
		public void StopPlayer() {
			textPlayTime.Text = "00:00 / 00:00";
			buttonPlay.Visibility = Visibility.Visible;
			buttonPause.Visibility = Visibility.Collapsed;
			rectPlayTime.Width = Pref.isPlaying = 0;

			textTitle.Text = "Simplayer4";
			textArtist.Text = Version; textAlbum.Text = "";
			imageAlbumart.Source = TagLibrary.rtSource("noImage.png");

			LyricsWindow.lS.Text = "";
			LyricsWindow.lO.Text = "Offset:0ms";
			LyricsWindow.lT.Text = "00:00 / 00:00";
			LyricsWindow.ChangeLabels("Simplayer4", "", "");

			if (Pref.ThemeCode == 6) {
				ChangeThemeColor(Colors.Black, false);
			}

			if (SongData.NowPlaying >= 0 && SongData.DictSong.ContainsKey(SongData.NowPlaying)) {
				//((Grid)SongData.DictSong[SongData.nNowPlaying].gBase.Children[4]).Visibility = Visibility.Collapsed;
				try {
					((Grid)SongData.DictSong[SongData.NowPlaying].GridBase.Children[4]).Children.Clear();
				} catch { }
			}
			SongData.NowPlaying = -1;

			mp.Stop();
		}

		// Get BitmapImage from uri
		public BitmapImage rtSource(string uriSource) {
			uriSource = "pack://application:,,,/Simplayer4;component/Resources/" + uriSource;
			BitmapImage source = new BitmapImage(new Uri(uriSource));
			return source;
		}

		// Divide hangul
		public string HangulDevide(string origStr) {
			string rtStr = "";
			for (int i = 0; i < origStr.Length; i++) {
				char origChar = origStr[i];
				if (origChar == ' ') { continue; }
				int unicode = Convert.ToInt32(origChar);

				uint jongCode = 0;
				uint jungCode = 0;
				uint choCode = 0;

				if (unicode < 44032 || unicode > 55203) {
					rtStr += origChar;
					continue;
				} else {
					uint uCode = Convert.ToUInt32(origChar - '\xAC00');
					jongCode = uCode % 28;
					jungCode = ((uCode - jongCode) / 28) % 21;
					choCode = ((uCode - jongCode) / 28) / 21;
				}
				char[] choChar = new char[] { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
				char[] jungChar = new char[] { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
				char[] jongChar = new char[] { ' ', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
				rtStr += choChar[choCode].ToString() + jungChar[jungCode].ToString() + jongChar[jongCode].ToString();
				rtStr = rtStr.Replace(" ", "");
			}
			return rtStr;
		}
	}
}
