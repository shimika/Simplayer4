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
		DispatcherTimer dtm = new DispatcherTimer() { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(200), };
		public void StartPlayingEvent() {
			dtm.Tick += dtm_Tick;
			dtm.Start();
		}

		public double dPlayPerTotal = 0;
		private void dtm_Tick(object sender, EventArgs e) {
			if (Pref.isPlaying == 0) { return; }

			TimeSpan nowPos = mp.Position; int min, sec;
			min = (int)nowPos.TotalMinutes; sec = nowPos.Seconds;
			string strBackup = textPlayTime.Text;
			textPlayTime.Text = LyricsWindow.lT.Text = string.Format("{0}:{1:D2} / {2}:{3:D2}", min, sec, (int)nowPlayingData.Duration.TotalMinutes, nowPlayingData.Duration.Seconds);

			dPlayPerTotal = mp.Position.TotalSeconds / nowPlayingData.Duration.TotalSeconds;
			if (strBackup != textPlayTime.Text) {
				rectPlayTime.Width = rectTotalTime.ActualWidth * dPlayPerTotal;
			}

			LyricsWindow.GetPlayTime(mp.Position);
		}



		public MediaPlayer mp = new MediaPlayer();
		public bool isReady = false;
		public bool MusicPrepare(int nId, int playType, bool isShowPreview, bool isForced = false) {
			if (!isForced) {
				if (isReady) { return false; }

				// 다음 음악 재생까지 0.5초 대기 (미리보기 창을 띄울경우)
				// 0.2초 대기 (통상 선택의 경우)

				isReady = true;
				DispatcherTimer dtm = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(isShowPreview ? 350 : 200), IsEnabled = true };
				dtm.Tick += (o, e) => {
					(o as DispatcherTimer).Stop();
					isReady = false;
				};
			}

			// 노래가 하나도 없다면 반환

			if (SongData.DictSong.Count == 0) {
				StopPlayer();
				return false;
			}

			// 직접 곡을 선택했는지, 아니면 그냥 재생을 눌렀는지

			if (nId == -1) {
				ShuffleList();
				PlayMusic(PositionArray[ShuffleArray[0]], 1, false);
			} else {
				if (!SongData.DictSong.ContainsKey(nId)) { return false; }
				int idx = 0;
				if (Math.Abs(playType) == 2) {
					if (!SongData.DictSong.ContainsKey(nId)) {
						idx = 0;
					} else {
						idx = Array.IndexOf(ShuffleArray, SongData.DictSong[nId].Position);
					}
				} else {
					if (!SongData.DictSong.ContainsKey(nId)) {
						idx = 0;
					} else {
						idx = SongData.DictSong[nId].Position;
					}
				}
				int n = SongData.DictSong.Count;

				// +/- : 음수는 이전, 양수는 다음 곡
				// 1은 순차, 2는 랜덤

				switch (playType) {
					case -2: PlayMusic(PositionArray[ShuffleArray[(idx + n - 1) % n]], -1, isShowPreview); break;
					case -1: PlayMusic(PositionArray[(idx + n - 1) % n], -1, isShowPreview); break;
					case 0: PlayMusic(nId, 1, isShowPreview); break;
					case 1: PlayMusic(PositionArray[(idx + 1) % n], 1, isShowPreview); break;
					case 2: PlayMusic(PositionArray[ShuffleArray[(idx + 1) % n]], 1, isShowPreview); break;
				}
			}
			return true;
		}

		SongData nowPlayingData = new SongData();
		public void PlayMusic(int nId, int nDirection, bool isShowPreview) {
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
				MusicPrepare(SongData.NowPlaying, Pref.PlayingLoopSeed * nDirection, false, true);
				return;
			}
			nowPlayingData = getSongData;

			((Polygon)SongData.DictSong[SongData.NowPlaying].GridBase.Children[5]).Visibility = Visibility.Collapsed;

			mp.MediaEnded += delegate(object sender, EventArgs e) { MusicPrepare(SongData.NowPlaying, Pref.PlayingLoopSeed * Pref.RandomSeed, false); };
			mp.MediaFailed += delegate(object sender, ExceptionEventArgs e) { MusicPrepare(SongData.NowPlaying, Pref.PlayingLoopSeed * nDirection, false, true); };
			mp.Open(new Uri(SongData.DictSong[nId].FilePath)); mp.Play();
			Pref.isPlaying = 1;

			buttonPlay.Visibility = Visibility.Collapsed;
			buttonPause.Visibility = Visibility.Visible;

			imageAlbumart.Source = nowPlayingData.AlbumArt;
			textTitle.Text = nowPlayingData.Title;
			textArtist.Text = nowPlayingData.Artist;
			textAlbum.Text = nowPlayingData.Album;

			// 앨범아트 컬러 색상 추출
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
				ChangeNotiWindow.textBefore.Text = SongData.DictSong[SongData.NowPlaying].Title;
				ChangeNotiWindow.textAfter.Text = nowPlayingData.Title;

				TitleTree.DeleteFromTree(SongData.NowPlaying);
				int nHeaderIndex = GetIndexerHeaderFrom(nowPlayingData.Title);

				SongData.DictSong[SongData.NowPlaying].Title = nowPlayingData.Title;
				SongData.DictSong[SongData.NowPlaying].SortTag = string.Format("{0:D4}{1}", nHeaderIndex, nowPlayingData.Title);

				TitleTree.AddToTree(SongData.NowPlaying);

				ChangeNotiWindow.textScript.Text = "";
				if (Pref.isAutoSort) {
					SortList();
					ShuffleList();

					ChangeNotiWindow.textScript.Text = "자동으로 정렬되었습니다.";
				} else {
					if (Pref.isSorted) {
						Pref.isSorted = false;
						gridIndexer.Visibility = Visibility.Collapsed;
						buttonIndexerSort.Visibility = Visibility.Visible;
						ChangeNotiWindow.textScript.Text = "리스트 정렬이 해제되었습니다. 인덱서가 비활성화됩니다.";
					}
				}
				SaveSongList();
				SavePreference();
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
			mp.Pause();
		}

		public void ResumeMusic() {
			buttonPlay.Visibility = Visibility.Collapsed;
			buttonPause.Visibility = Visibility.Visible; 
			Pref.isPlaying = 1; 
			mp.Play();
		}
	}
}
