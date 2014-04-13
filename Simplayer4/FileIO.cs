using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Simplayer4 {
	public class FileIO {
		public static string ffIcon = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4.ico";
		static string ffList = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4.ini";
		static string ffPref = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Simplayer4Pref.ini";
		static string ffPrev = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SimplayerFLAT.ini";
		public static DispatcherTimer dtmSave;

		public static void ReadPreference() {
			dtmSave = new DispatcherTimer() { Interval = TimeSpan.FromMinutes(1), IsEnabled = false };
			dtmSave.Tick += dtmSave_Tick;

			/*
			if (!File.Exists(ffIcon)) {
				string imagePath = "pack://application:,,,/Simplayer4;component/Resources/iconMP3.ico";
				var info = Application.GetResourceStream(new Uri(imagePath));
				var memStream = new MemoryStream();
				info.Stream.CopyTo(memStream);

				File.WriteAllBytes(ffIcon, memStream.ToArray());
			}
			 */ 

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
							Pref.nRandomSeed = 2;
						} else {
							Pref.nRandomSeed = 1;
						}
						break;
					case "playall":
						if (Convert.ToBoolean(strInnerSet[1])) {
							Pref.nPlayingLoopSeed = 1;
						} else {
							Pref.nPlayingLoopSeed = 0;
						}
						break;
					case "oneclick": Pref.isOneClickPlaying = Convert.ToBoolean(strInnerSet[1]); break;
					case "volume": Pref.nVolume = Convert.ToInt32(strInnerSet[1]); break;
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
							Pref.nTheme = Math.Min(Convert.ToInt32(strInnerSet[1]), 6);
						} else {
							int r = Convert.ToInt32(strSplit[0]);
							int g = Convert.ToInt32(strSplit[1]);
							int b = Convert.ToInt32(strSplit[2]);

							Pref.nTheme = 7;
							Pref.cTheme = Color.FromRgb((byte)r, (byte)g, (byte)b);
						}
						break;
				}
			}
		}

		public static string strIndexCaption = "0123456789ㄱㄲㄴㄷㄸㄹㅁㅂㅃㅅㅆㅇㅈㅉㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZぁァあアぃィいイぅゥうウぇェえエぉォおオかカがガきキぎギくクぐグけケげゲこコごゴさサざザしシじジすスずズせセぜゼそソぞゾたタだダちチぢヂっッつツづヅてテでデとトどドなナにニぬヌねネのノはハばバぱパひヒびビぴピふフぶブぷプへヘべベぺペほホぼボぽポまマみミむムめメもモゃャやヤゅュゆユょョよヨらラりリるルれレろロゎヮわワをヲんンヴ―#";
		public static string strIndexValue = "1111111111ㄱㄱㄴㄷㄷㄹㅁㅂㅂㅅㅅㅇㅈㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZああああああああああああああああああああかかかかかかかかかかかかかかかかかかかかささささささささささささささささささささたたたたたたたたたたたたたたたたたたたたたたななななななななななははははははははははははははははははははははははははははははままままままままままややややややややややややららららららららららわわわわわわわわわわ#";
		public static string strIndexUnique = "1ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎABCDEFGHIJKLMNOPQRSTUVWXYZあかさたなはまやらわ#";

		public static List<SongData> ReadSongList() {
			List<SongData> listSong = new List<SongData>();
			if (!File.Exists(ffList)) {
				if (File.Exists(ffPrev)) {
					File.Copy(ffPrev, ffList);
				} else {
					StreamWriter sw = new StreamWriter(ffList);
					sw.Write(""); sw.Flush(); sw.Close();
				}
			}

			StreamReader sr = new StreamReader(ffList);
			string strList = sr.ReadToEnd(); sr.Close();

			foreach (string str in strList.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)) {
				string[] strInnerSet = str.Split(new string[] { "__simplayer__" }, StringSplitOptions.RemoveEmptyEntries);

				SongData sData = new SongData() {
					strFilePath = strInnerSet[0],
					strTitle = strInnerSet[1],
					strDuration = strInnerSet[2],
					nID = SongData.nCount,
				};

				char cHead = HangulDevide(sData.strTitle.ToUpper())[0];
				int idx = strIndexCaption.IndexOf(cHead);
				if (idx < 0) { idx += strIndexCaption.Length; }

				sData.strSortTag = idx.ToString("000") + sData.strTitle;
				sData.nHeadIndex = strIndexUnique.IndexOf(strIndexValue[idx]);

				listSong.Add(sData);
				SongData.DictSong.Add(SongData.nCount, sData);
				SongData.nCount++;
			}

			if (Pref.isAutoSort) {
				listSong.Sort(new SortByValue());
			}
			return listSong;
		}

		public static void SaveSongList() {
			using (StreamWriter sw = new StreamWriter(ffList)) {
				foreach (int nIndex in PlayClass.nPositionArray) {
					SongData sData = SongData.DictSong[nIndex];
					sw.WriteLine(sData.strFilePath + "__simplayer__" + sData.strTitle + "__simplayer__" + sData.strDuration);
				}
			}
		}

		public static void dtmSave_Tick(object sender, EventArgs e) {
			Pref.nVolume = (int)(PlayClass.mp.Volume * 50);
			SavePreference();
		}

		public static void SavePreference() {
			using (StreamWriter sw = new StreamWriter(ffPref)) {
				sw.WriteLine(string.Format("autosort={0}", Pref.isAutoSort));
				sw.WriteLine(string.Format("list={0}", Pref.isListVisible));
				sw.WriteLine(string.Format("lyrics={0}", Pref.isLyricsVisible));
				sw.WriteLine(string.Format("notify={0}", Pref.isNofifyOn));
				sw.WriteLine(string.Format("oneclick={0}", Pref.isOneClickPlaying));
				sw.WriteLine(string.Format("sorted={0}", Pref.isSorted));
				sw.WriteLine(string.Format("playall={0}", Pref.nPlayingLoopSeed == 1 ? true : false));
				sw.WriteLine(string.Format("random={0}", Pref.nRandomSeed == 2 ? true : false));
				sw.WriteLine(string.Format("tray={0}", Pref.isTray));
				sw.WriteLine(string.Format("hotkey={0}", Pref.isHotkeyOn));
				sw.WriteLine(string.Format("topmost={0}", Pref.isTopMost));
				sw.WriteLine(string.Format("volume={0}", (int)Pref.nVolume));
				sw.WriteLine(string.Format("lyrright={0}",Pref.isLyricsRight));
				if (Pref.nTheme < 7) {
					sw.WriteLine(string.Format("theme={0}", (int)Pref.nTheme));
				} else {
					sw.WriteLine(string.Format("theme={0},{1},{2}", (int)Pref.cTheme.R, (int)Pref.cTheme.G, (int)Pref.cTheme.B));
				}
			}
		}

		public class SortByValue : IComparer<SongData> {
			public int Compare(SongData arg1, SongData arg2) {
				if (arg1.strSortTag.IndexOf(arg2.strSortTag) == 0) { return 1; }
				if (arg2.strSortTag.IndexOf(arg1.strSortTag) == 0) { return -1; }
				return string.Compare(arg1.strSortTag, arg2.strSortTag);
			}
		}

		public static string HangulDevide(string origStr) {
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
