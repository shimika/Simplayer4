using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		DispatcherTimer PlayingTimer = new DispatcherTimer() { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(200), };
		public void StartPlayingEvent() {
			PlayingTimer.Tick += PlayingTimer_Tick;
			PlayingTimer.Start();
		}

		double PlayPerTotal = 0;
		private void PlayingTimer_Tick(object sender, EventArgs e) {
			if (Pref.isPlaying == 0) { return; }
			
			TimeSpan nowPos = MusicPlayer.Position; int min, sec;
			min = (int)nowPos.TotalMinutes; sec = nowPos.Seconds;
			string strBackup = textPlayTime.Text;
			textPlayTime.Text = LyricsWindow.lT.Text = string.Format("{0}:{1:D2} / {2}:{3:D2}", min, sec, (int)nowPlayingData.Duration.TotalMinutes, nowPlayingData.Duration.Seconds);

			PlayPerTotal = MusicPlayer.Position.TotalSeconds / nowPlayingData.Duration.TotalSeconds;

			if (strBackup != textPlayTime.Text) {
				rectPlayTime.Width = rectTotalTime.ActualWidth * PlayPerTotal;
			}

			LyricsWindow.GetPlayTime(MusicPlayer.Position);
		}

		public MediaPlayer MusicPlayer = new MediaPlayer();
		public bool isDelay = false;
		public DispatcherTimer TimerDelay = new DispatcherTimer();

		public bool MusicPrepare(int id, int playType, bool isShowPreview, bool isForced = false) {
			if (!isForced) {
				if (isDelay) { return false; }

				// if preview window on, delay 350ms
				// else				     delay 200ms

				isDelay = true;
				TimerDelay.Interval = TimeSpan.FromMilliseconds(isShowPreview ? 350 : 200);
				TimerDelay.Start();
			}

			// return 0 if song does'nt exists

			if (SongData.DictSong.Count == 0) {
				StopPlayer();
				return false;
			}

			// if select specific, nID is positive
			// else ID is negative

			if (id == -1) {
				ShuffleList();
				PlayingDirection = 1;
				PlayMusic(PositionArray[ShuffleArray[0]], false);
			} else {
				if (!SongData.DictSong.ContainsKey(id)) { return false; }
				int idx = 0;
				if (Math.Abs(playType) == 2) {
					if (!SongData.DictSong.ContainsKey(id)) {
						idx = 0;
					} else {
						idx = Array.IndexOf(ShuffleArray, SongData.DictSong[id].Position);
					}
				} else {
					if (!SongData.DictSong.ContainsKey(id)) {
						idx = 0;
					} else {
						idx = SongData.DictSong[id].Position;
					}
				}
				int n = SongData.DictSong.Count;

				// positive = next, negative = prev
				// linear = 1, random = 2

				if (playType >= 0) {
					PlayingDirection = 1;
				} else {
					PlayingDirection = -1;
				}

				switch (playType) {
					case -2: PlayMusic(PositionArray[ShuffleArray[(idx + n - 1) % n]], isShowPreview); break;
					case -1: PlayMusic(PositionArray[(idx + n - 1) % n], isShowPreview); break;
					case 0: PlayMusic(id, isShowPreview); break;
					case 1: PlayMusic(PositionArray[(idx + 1) % n], isShowPreview); break;
					case 2: PlayMusic(PositionArray[ShuffleArray[(idx + 1) % n]], isShowPreview); break;
				}
			}
			return true;
		}

		SongData nowPlayingData = new SongData();
		int PlayingDirection = 1;

		public void PlayMusic(int nId, bool isShowPreview) {
			if (SongData.NowPlaying >= 0 && SongData.DictSong.ContainsKey(SongData.NowPlaying)) {
				try {
					((Grid)SongData.DictSong[SongData.NowPlaying].GridBase.Children[4]).Children.Clear();
				} catch { }
			}

			SongData.NowPlaying = nId;

			if (!SongData.DictSong.ContainsKey(SongData.NowPlaying)) {
				MusicPrepare(-1, 1, true);
				return;
			}
			
			SongData getSongData = new SongData() { FilePath = SongData.DictSong[SongData.NowPlaying].FilePath, };
			bool isOK = TagLibrary.InsertTagInDatabase(ref getSongData, false);
			if (!isOK) {
				if (SongData.DictSong.ContainsKey(SongData.NowPlaying)) {
					((TextBlock)SongData.DictSong[SongData.NowPlaying].GridBase.Children[0]).TextDecorations = TextDecorations.Strikethrough;
				}
				MusicPrepare(SongData.NowPlaying, Pref.PlayingLoopSeed * PlayingDirection, false, true);
				return;
			}

			nowPlayingData = getSongData;
			((Polygon)SongData.DictSong[SongData.NowPlaying].GridBase.Children[5]).Visibility = Visibility.Collapsed;

			MusicPlayer.Open(new Uri(SongData.DictSong[nId].FilePath));
			ResumeMusic();

			imageAlbumart.Source = nowPlayingData.AlbumArt;
			textTitle.Text = nowPlayingData.Title;
			textArtist.Text = nowPlayingData.Artist;
			textAlbum.Text = nowPlayingData.Album;

			// Extract albumart color
			if (Pref.ThemeCode == 6) {
				Color c = TagLibrary.CalculateAverageColor(nowPlayingData.AlbumArt);
				ChangeThemeColor(c, false);
			}

			textPlayTime.Text = string.Format("0:00 / {0}:{1:D2}", (int)nowPlayingData.Duration.TotalMinutes, nowPlayingData.Duration.Seconds);
			rectPlayTime.Width = 0;

			string noti = nowPlayingData.Title;
			if (noti.Length > 60) { noti = noti.Substring(0, 60) + "..."; }
			TrayNotify.Text = noti.Replace('&', '＆');

			((Grid)SongData.DictSong[SongData.NowPlaying].GridBase.Children[4]).Children.Add(GridNowPlay);
			((TextBlock)SongData.DictSong[SongData.NowPlaying].GridBase.Children[0]).TextDecorations = null;

			if (SongData.DictSong[SongData.NowPlaying].Title != nowPlayingData.Title) {
				string before = SongData.DictSong[SongData.NowPlaying].Title;
				string after = nowPlayingData.Title;
				string sortinfo = "자동으로 정렬되었습니다.";

				int nHeaderIndex = GetIndexerHeaderFrom(nowPlayingData.Title);

				SongData.DictSong[SongData.NowPlaying].Title = nowPlayingData.Title;
				SongData.DictSong[SongData.NowPlaying].SortTag = string.Format("{0:D4}{1}", nHeaderIndex, nowPlayingData.Title);

				TitleTree.RefreshTagDB();

				if (Pref.isAutoSort) {
					SortList();
					ShuffleList();
				} else {
					if (Pref.isSorted) {
						Pref.isSorted = false;
						gridIndexer.Visibility = Visibility.Collapsed;
						buttonIndexerSort.Visibility = Visibility.Visible;

						sortinfo = "리스트 정렬이 해제되었습니다. 인덱서가 비활성화됩니다.";
					}
				}
				SaveSongList();
				SavePreference();

				ChangeNotification ChangeNotiWindow = new ChangeNotification(before, after, sortinfo);
				ChangeNotiWindow.Show();
				ChangeNotiWindow.AnimateWindow();
			}

			if (isShowPreview && Pref.isNofifyOn && !Pref.isLyricsVisible) {
				//pWindow.AnimateWindow(string.Format("{0}{1}{2}", , );
				PrevWindow.AnimateWindow(nowPlayingData.Title, nowPlayingData.Artist == "" ? "" : " " + nowPlayingData.Artist);
			} else {
				PrevWindow.AnimateWindow(nowPlayingData.Title, nowPlayingData.Artist == "" ? "" : " " + nowPlayingData.Artist, false);
			}

			LyricsWindow.InitLyrics(nowPlayingData);
		}

		public void PauseMusic() {
			buttonPlay.Visibility = Visibility.Visible;
			buttonPause.Visibility = Visibility.Collapsed;
			Pref.isPlaying = -1; 
			MusicPlayer.Pause();
		}

		public void ResumeMusic() {
			buttonPlay.Visibility = Visibility.Collapsed;
			buttonPause.Visibility = Visibility.Visible; 
			Pref.isPlaying = 1; 
			MusicPlayer.Play();
		}
	}
}
