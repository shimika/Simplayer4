using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Simplayer4 {
	public class SongData : INotifyPropertyChanged {
		public int nID, nPosition, nHeadIndex;
		public string strArtist, strAlbum, strFilePath, strSortTag, strDuration;
		public BitmapImage imgArt;
		public Grid gBase;
		public TimeSpan Duration;
		public bool isExists = true;

		private string _strTitle;
		public string strTitle {
			get { return _strTitle; }
			set {
				_strTitle = value;
				OnPropertyChanged("strTitle");
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public static int nCount = 0, nNowSelected = -1, nNowPlaying = -1;
		public static Dictionary<int, SongData> DictSong = new Dictionary<int, SongData>();
	}
	public class Pref {
		public static bool isLyricsVisible, isListVisible, isOneClickPlaying, isAutoSort, isNofifyOn, isSorted, isTray, isHotkeyOn, isTopMost, isLyricsRight;
		public static int nRandomSeed, nPlayingLoopSeed, nTheme;

		public static bool isShowing = true, isPrefVisible = false;
		public static int isPlaying = 0;
		public static double nVolume, nListHeight;
		public static Color cTheme;
	}
}
