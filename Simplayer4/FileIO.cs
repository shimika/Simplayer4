using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		public string ffIcon = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4.ico";
		string ffPrevList = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4.ini";
		string ffPrevPref = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4Pref.ini";

		string ffList = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4\Simplayer4.ini";
		string ffPref = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4\Simplayer4Pref.ini";

		string ffFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4";

		public void ReadPreference() {
			VolumeSaveTimer.Tick += VolumeSaveTimer_Tick;
			VolumeSaveTimer.Start();

			// Make directory
			if (!Directory.Exists(ffFolder)) { Directory.CreateDirectory(ffFolder); }

			// Move previous data
			if (File.Exists(ffPrevList)) { File.Move(ffPrevList, ffList); }
			if (File.Exists(ffPrevPref)) { File.Move(ffPrevPref, ffPref); }

			if (!File.Exists(ffList)) {
				StreamWriter sw = new StreamWriter(ffList);
				sw.Write(""); sw.Flush(); sw.Close();
			}

			if (!File.Exists(ffPref)) {
				StreamWriter sw = new StreamWriter(ffPref);
				sw.WriteLine("lyrics=False");
				sw.WriteLine("list=True");
				sw.WriteLine("oneclick=False");
				sw.WriteLine("autosort=False");
				sw.WriteLine("sorted=False");
				sw.WriteLine("notify=True");
				sw.WriteLine("random=False");
				sw.WriteLine("playall=True");
				sw.WriteLine("tray=false");
				sw.WriteLine("hotkey=false");
				sw.WriteLine("topmost=false");
				sw.WriteLine("volume=50");
				sw.WriteLine("lyrright=true");
				sw.WriteLine("theme=5");
				sw.Flush(); sw.Close();
			}

			StreamReader sr = new StreamReader(ffPref);
			string strSetting = sr.ReadToEnd(); sr.Close();
			foreach (string str in strSetting.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
				string[] strInnerSet = str.Split('=');
				switch (strInnerSet[0]) {
					case "lyrics": Pref.isLyricsVisible = Convert.ToBoolean(strInnerSet[1]); break;
					case "list": Pref.isListVisible = Convert.ToBoolean(strInnerSet[1]); break;
					case "random":
						if (Convert.ToBoolean(strInnerSet[1])) {
							Pref.RandomSeed = 2;
						} else {
							Pref.RandomSeed = 1;
						}
						break;
					case "playall":
						if (Convert.ToBoolean(strInnerSet[1])) {
							Pref.PlayingLoopSeed = 1;
						} else {
							Pref.PlayingLoopSeed = 0;
						}
						break;
					case "oneclick": Pref.isOneClickPlaying = Convert.ToBoolean(strInnerSet[1]); break;
					case "volume": Pref.Volume = Convert.ToInt32(strInnerSet[1]); break;
					case "autosort": Pref.isAutoSort = Convert.ToBoolean(strInnerSet[1]); break;
					case "sorted": Pref.isSorted = Convert.ToBoolean(strInnerSet[1]); break;
					case "tray": Pref.isTray = Convert.ToBoolean(strInnerSet[1]); break;
					case "hotkey": Pref.isHotkeyOn = Convert.ToBoolean(strInnerSet[1]); break;
					case "topmost": Pref.isTopMost = Convert.ToBoolean(strInnerSet[1]); break;
					case "notify": Pref.isNofifyOn = Convert.ToBoolean(strInnerSet[1]); break;
					case "lyrright": Pref.isLyricsRight = Convert.ToBoolean(strInnerSet[1]); break;
					case "theme":
						string[] strSplit = strInnerSet[1].Split(',');
						if (strSplit.Length == 1) {
							Pref.ThemeCode = Math.Min(Convert.ToInt32(strInnerSet[1]), 6);
						} else {
							int r = Convert.ToInt32(strSplit[0]);
							int g = Convert.ToInt32(strSplit[1]);
							int b = Convert.ToInt32(strSplit[2]);

							Pref.ThemeCode = 7;
							Pref.ThemeColor = Color.FromRgb((byte)r, (byte)g, (byte)b);
						}
						break;
				}
			}
		}

		public List<SongData> ReadSongList() {
			List<SongData> listInput = new List<SongData>();

			StreamReader sr = new StreamReader(ffList);
			string strList = sr.ReadToEnd(); sr.Close();

			foreach (string str in strList.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
				string[] strInnerSet = str.Split(new string[] { "__simplayer__" }, StringSplitOptions.RemoveEmptyEntries);

				SongData sData = new SongData() {
					FilePath = strInnerSet[0],
					Title = strInnerSet[1],
					DurationString = strInnerSet[2],
				};

				listInput.Add(sData);
			}

			return listInput;
		}

		public void SaveSongList() {
			using (StreamWriter sw = new StreamWriter(ffList)) {
				foreach (SongData sData in ListSong) {
					sw.WriteLine(sData.FilePath + "__simplayer__" + sData.Title + "__simplayer__" + sData.DurationString);
				}
			}
		}

		public DispatcherTimer VolumeSaveTimer = new DispatcherTimer() { Interval = TimeSpan.FromMinutes(1), IsEnabled = false };
		public void VolumeSaveTimer_Tick(object sender, EventArgs e) {
			Pref.Volume = (int)(mp.Volume * 50);
			SavePreference();
		}

		public void SavePreference() {
			using (StreamWriter sw = new StreamWriter(ffPref)) {
				sw.WriteLine(string.Format("autosort={0}", Pref.isAutoSort));
				sw.WriteLine(string.Format("list={0}", Pref.isListVisible));
				sw.WriteLine(string.Format("lyrics={0}", Pref.isLyricsVisible));
				sw.WriteLine(string.Format("notify={0}", Pref.isNofifyOn));
				sw.WriteLine(string.Format("oneclick={0}", Pref.isOneClickPlaying));
				sw.WriteLine(string.Format("sorted={0}", Pref.isSorted));
				sw.WriteLine(string.Format("playall={0}", Pref.PlayingLoopSeed == 1 ? true : false));
				sw.WriteLine(string.Format("random={0}", Pref.RandomSeed == 2 ? true : false));
				sw.WriteLine(string.Format("tray={0}", Pref.isTray));
				sw.WriteLine(string.Format("hotkey={0}", Pref.isHotkeyOn));
				sw.WriteLine(string.Format("topmost={0}", Pref.isTopMost));
				sw.WriteLine(string.Format("volume={0}", (int)Pref.Volume));
				sw.WriteLine(string.Format("lyrright={0}",Pref.isLyricsRight));
				if (Pref.ThemeCode < 7) {
					sw.WriteLine(string.Format("theme={0}", (int)Pref.ThemeCode));
				} else {
					sw.WriteLine(string.Format("theme={0},{1},{2}", (int)Pref.ThemeColor.R, (int)Pref.ThemeColor.G, (int)Pref.ThemeColor.B));
				}
			}
		}
	}
}
