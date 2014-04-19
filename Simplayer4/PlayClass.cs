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
	public class PlayClass {
		static DispatcherTimer dtm = new DispatcherTimer() { IsEnabled = true, Interval = TimeSpan.FromMilliseconds(100), };
		public static void StartPlayer() {
			dtm.Tick += dtm_Tick;
			dtm.Start();
		}

		public static double dPlayPerTotal = 0;
		private static void dtm_Tick(object sender, EventArgs e) {
			if (Pref.isPlaying == 0) { return; }

			TimeSpan nowPos = mp.Position; int min, sec;
			min = (int)nowPos.TotalMinutes; sec = nowPos.Seconds;
			string strBackup = winMain.textPlayTime.Text;
			winMain.textPlayTime.Text = winMain.lyrWindow.lT.Text = string.Format("{0}:{1:D2} / {2}:{3:D2}", min, sec, (int)nowPlayingData.Duration.TotalMinutes, nowPlayingData.Duration.Seconds);

			dPlayPerTotal = mp.Position.TotalSeconds / nowPlayingData.Duration.TotalSeconds;
			if (strBackup != winMain.textPlayTime.Text) {
				winMain.rectPlayTime.Width = winMain.rectTotalTime.ActualWidth * dPlayPerTotal;
			}

			winMain.lyrWindow.GetPlayTime(mp.Position);
		}

		public static void RefreshSongPosition() {
			int nIndex = 0;
			for (int i = 0; i < winMain.stackList.Children.Count; i++) {
				Grid grid = (Grid)winMain.stackList.Children[i];
				nIndex = (int)grid.Tag;
				SongData.DictSong[nIndex].nPosition = i;
			}
		}

		public static int[] nPositionArray, nShuffleArray;
		public static void ShuffleList() {
			RefreshSongPosition();

			nPositionArray = new int[SongData.DictSong.Count];
			nShuffleArray = new int[SongData.DictSong.Count];

			foreach (KeyValuePair<int, SongData> sData in SongData.DictSong) {
				nPositionArray[sData.Value.nPosition] = sData.Key;
			}
			for (int i = 0; i < SongData.DictSong.Count; i++) {
				nShuffleArray[i] = i;
			}

			Random random = new Random();
			for (int k = 0; k < 3; k++) {
				for (int i = SongData.DictSong.Count - 1; i >= 0; i--) {
					int j = random.Next(i);
					int t = nShuffleArray[i]; nShuffleArray[i] = nShuffleArray[j]; nShuffleArray[j] = t;
				}
			}
		}

		public static MediaPlayer mp = new MediaPlayer();
		public static bool isReady = false;
		public static bool MusicPrepare(int nId, int playType, bool isShowPreview, bool isForced = false) {
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
				winMain.StopPlayer();
				return false;
			}

			// 직접 곡을 선택했는지, 아니면 그냥 재생을 눌렀는지

			if (nId == -1) {
				ShuffleList();
				PlayMusic(nPositionArray[nShuffleArray[0]], 1, false);
			} else {
				if (!SongData.DictSong.ContainsKey(nId)) { return false; }
				int idx = 0;
				if (Math.Abs(playType) == 2) {
					if (!SongData.DictSong.ContainsKey(nId)) {
						idx = 0;
					} else {
						idx = Array.IndexOf(nShuffleArray, SongData.DictSong[nId].nPosition);
					}
				} else {
					if (!SongData.DictSong.ContainsKey(nId)) {
						idx = 0;
					} else {
						idx = SongData.DictSong[nId].nPosition;
					}
				}
				int n = SongData.DictSong.Count;

				// +/- : 음수는 이전, 양수는 다음 곡
				// 1은 순차, 2는 랜덤

				switch (playType) {
					case -2: PlayMusic(nPositionArray[nShuffleArray[(idx + n - 1) % n]], -1, isShowPreview); break;
					case -1: PlayMusic(nPositionArray[(idx + n - 1) % n], -1, isShowPreview); break;
					case 0: PlayMusic(nId, 1, isShowPreview); break;
					case 1: PlayMusic(nPositionArray[(idx + 1) % n], 1, isShowPreview); break;
					case 2: PlayMusic(nPositionArray[nShuffleArray[(idx + 1) % n]], 1, isShowPreview); break;
				}
			}
			return true;
		}

		public static MainWindow winMain;
		static SongData nowPlayingData = new SongData();

		public static void PlayMusic(int nId, int nDirection, bool isShowPreview) {
			if (SongData.nNowPlaying >= 0 && SongData.DictSong.ContainsKey(SongData.nNowPlaying)) {
				//((Grid)SongData.DictSong[SongData.nNowPlaying].gBase.Children[4]).Visibility = Visibility.Collapsed;
				try {
					((Grid)SongData.DictSong[SongData.nNowPlaying].gBase.Children[4]).Children.Clear();
				} catch { }
			}
			SongData.nNowPlaying = nId;

			if (!SongData.DictSong.ContainsKey(SongData.nNowPlaying)) {
				MusicPrepare(-1, 1, true);
				return;
			}
			
			SongData getSongData = new SongData() { strFilePath = SongData.DictSong[SongData.nNowPlaying].strFilePath, };
			bool isOK = TagLibrary.InsertTagInDatabase(getSongData);
			if (!isOK) {
				if (SongData.DictSong.ContainsKey(SongData.nNowPlaying)) {
					((TextBlock)SongData.DictSong[SongData.nNowPlaying].gBase.Children[0]).TextDecorations = TextDecorations.Strikethrough;
				}
				MusicPrepare(SongData.nNowPlaying, Pref.nPlayingLoopSeed * nDirection, false, true);
				return;
			}
			nowPlayingData = getSongData;

			((Polygon)SongData.DictSong[SongData.nNowPlaying].gBase.Children[5]).Visibility = Visibility.Collapsed;

			mp.MediaEnded += delegate(object sender, EventArgs e) { MusicPrepare(SongData.nNowPlaying, Pref.nPlayingLoopSeed * Pref.nRandomSeed, false); };
			mp.MediaFailed += delegate(object sender, ExceptionEventArgs e) { MusicPrepare(SongData.nNowPlaying, Pref.nPlayingLoopSeed * nDirection, false, true); };
			mp.Open(new Uri(SongData.DictSong[nId].strFilePath)); mp.Play();
			Pref.isPlaying = 1;

			winMain.buttonPlay.Visibility = Visibility.Collapsed;
			winMain.buttonPause.Visibility = Visibility.Visible;

			winMain.imageAlbumart.Source = nowPlayingData.imgArt;
			winMain.textTitle.Text = nowPlayingData.strTitle;
			winMain.textArtist.Text = nowPlayingData.strArtist;
			winMain.textAlbum.Text = nowPlayingData.strAlbum;

			// 앨범아트 컬러 색상 추출
			if (Pref.nTheme == 6) {
				Color c = TagLibrary.CalculateAverageColor(nowPlayingData.imgArt);
				winMain.ChangeThemeColor(c, false);
			}

			winMain.textPlayTime.Text = string.Format("0:00 / {0}:{1:D2}", (int)nowPlayingData.Duration.TotalMinutes, nowPlayingData.Duration.Seconds);
			winMain.rectPlayTime.Width = 0;

			string noti = nowPlayingData.strTitle;
			if (noti.Length > 60) { noti = noti.Substring(0, 60) + "..."; }
			winMain.ni.Text = noti.Replace('&', '＆');

			//((Grid)SongData.DictSong[SongData.nNowPlaying].gBase.Children[4]).Visibility = Visibility.Visible;
			((Grid)SongData.DictSong[SongData.nNowPlaying].gBase.Children[4]).Children.Add(winMain.GridNowPlay);
			((TextBlock)SongData.DictSong[SongData.nNowPlaying].gBase.Children[0]).TextDecorations = null;

			if (SongData.DictSong[SongData.nNowPlaying].strTitle != nowPlayingData.strTitle) {
				winMain.cWindow.textBefore.Text = SongData.DictSong[SongData.nNowPlaying].strTitle;
				winMain.cWindow.textAfter.Text = nowPlayingData.strTitle;

				char cHead = FileIO.HangulDevide(nowPlayingData.strTitle.ToUpper())[0];
				int idx = FileIO.strIndexCaption.IndexOf(cHead);
				if (idx < 0) { idx += FileIO.strIndexCaption.Length; }

				SongData.DictSong[SongData.nNowPlaying].strTitle = nowPlayingData.strTitle;
				//((TextBlock)SongData.dictSong[SongData.nNowPlaying].gBase.Children[0]).Text = nowPlayingData.strTitle;

				SongData.DictSong[SongData.nNowPlaying].strSortTag = string.Format("{0:D3}{1}", idx, nowPlayingData.strTitle);

				winMain.cWindow.textScript.Text = "";
				if (Pref.isAutoSort) {
					ListOrder.ListSort();
					winMain.cWindow.textScript.Text = "자동으로 정렬되었습니다.";
				} else {
					if (Pref.isSorted) {
						Pref.isSorted = false;
						winMain.gridIndexer.Visibility = Visibility.Collapsed;
						winMain.buttonIndexerSort.Visibility = Visibility.Visible;
						winMain.cWindow.textScript.Text = "리스트 정렬이 해제되었습니다. 인덱서가 비활성화됩니다.";
					}
				}
				FileIO.SaveSongList();
				winMain.cWindow.AnimateWindow();
			}

			if (isShowPreview && Pref.isNofifyOn && !Pref.isLyricsVisible) {
				//winMain.pWindow.AnimateWindow(string.Format("{0}{1}{2}", , );
				winMain.pWindow.AnimateWindow(nowPlayingData.strTitle, nowPlayingData.strArtist == "" ? "" : " " + nowPlayingData.strArtist);
			} else {
				winMain.pWindow.AnimateWindow(nowPlayingData.strTitle, nowPlayingData.strArtist == "" ? "" : " " + nowPlayingData.strArtist, false);
			}

			winMain.lyrWindow.InitLyrics(nowPlayingData);
		}

		public static void PauseMusic() {
			winMain.buttonPlay.Visibility = Visibility.Visible;
			winMain.buttonPause.Visibility = Visibility.Collapsed;
			Pref.isPlaying = -1; 
			mp.Pause();
		}

		public static void ResumeMusic() {
			winMain.buttonPlay.Visibility = Visibility.Collapsed;
			winMain.buttonPause.Visibility = Visibility.Visible; 
			Pref.isPlaying = 1; 
			mp.Play();
		}
	}
}
