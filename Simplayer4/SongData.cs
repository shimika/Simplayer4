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
		public int ID, Position, SortPosition, HeadIndex;
		public string Artist, Album, FilePath, SortTag, DurationString;
		public BitmapImage AlbumArt;
		public Grid GridBase;
		public TimeSpan Duration;
		public bool isExists = true;

		private string _strTitle;
		public string Title {
			get { return _strTitle; }
			set {
				_strTitle = value;
				OnPropertyChanged("Title");
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null) {
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public static int IDCount = 0, NowSelected = -1, NowPlaying = -1;
		public static Dictionary<int, SongData> DictSong = new Dictionary<int, SongData>();
	}
	public class Pref {
		public static bool isLyricsVisible, isListVisible, isOneClickPlaying, isAutoSort, isNofifyOn, isSorted, isTray, isHotkeyOn, isTopMost, isLyricsRight;
		public static int RandomSeed, PlayingLoopSeed, ThemeCode;

		public static bool isShowing = true, isPrefVisible = false;
		public static int isPlaying = 0;
		public static double Volume, ListHeight;
		public static Color ThemeColor;
	}
}
