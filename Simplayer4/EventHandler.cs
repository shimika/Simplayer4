using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Simplayer4 {
	public partial class MainWindow : Window {

		#region API area

		[DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		const UInt32 SWP_NOSIZE = 0x0001;
		const UInt32 SWP_NOMOVE = 0x0002;

		static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		static void SendWpfWindowBack(Window window) {
			var hWnd = new WindowInteropHelper(window).Handle;
			SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
		}

		#endregion

		private void buttonMinimize_Click(object sender, RoutedEventArgs e) {
			if (Pref.isTray) {
				Pref.isShowing = false;

				Storyboard sb = new Storyboard();
				DoubleAnimation da = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200)) {
					EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut }
				};
				Storyboard.SetTarget(da, this);
				Storyboard.SetTargetProperty(da, new PropertyPath(Window.OpacityProperty));
				sb.Children.Add(da);
				sb.Completed += (o, ex) => {
					SendWpfWindowBack(this);
					this.WindowState = WindowState.Minimized;
				};
				sb.Begin(this);

				new AltTab().HideAltTab(this);
			} else {
				this.WindowState = WindowState.Minimized;
			}
		}

		private void MainWindow_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
			if (OneTimeCancel || ReArrange.isMouseDown) {
				OneTimeCancel = false;
				e.Handled = true;
			} else {
				gridContextBlock.Visibility = Visibility.Visible;
			}
		}

		private void buttonPref_Click(object sender, RoutedEventArgs e) {
			Pref.isPrefVisible = !Pref.isPrefVisible;
			imagePref.Visibility = Pref.isPrefVisible ? Visibility.Collapsed : Visibility.Visible;
			imagePrefBack.Visibility = Pref.isPrefVisible ? Visibility.Visible : Visibility.Collapsed;
			gridPrefBackground.IsHitTestVisible = Pref.isPrefVisible;

			double dDelay = 0, nValue = Pref.isPrefVisible ? 1 : 0;
			if (!Pref.isListVisible && Pref.isPrefVisible) {
				ToggleList();
				dDelay = 300;
			}

			Storyboard sb = new Storyboard();
			DoubleAnimation daPref = new DoubleAnimation(nValue, TimeSpan.FromMilliseconds(400));
			ThicknessAnimation taPref = new ThicknessAnimation(new Thickness(0, -gridPref.ActualHeight * nValue, 0, 0), new Thickness(0, -gridPref.ActualHeight * (1 - nValue), 0, 0), TimeSpan.FromMilliseconds(300)) {
				BeginTime = TimeSpan.FromMilliseconds(100),
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
			};
			Storyboard.SetTarget(daPref, gridPrefBackground); Storyboard.SetTargetProperty(daPref, new PropertyPath(Grid.OpacityProperty));
			Storyboard.SetTarget(taPref, gridPref); Storyboard.SetTargetProperty(taPref, new PropertyPath(Grid.MarginProperty));

			sb.Children.Add(daPref);
			sb.Children.Add(taPref);
			sb.BeginTime = TimeSpan.FromMilliseconds(dDelay);
			sb.Begin(this);
		}

		private void buttonPrev_Click(object sender, RoutedEventArgs e) {
			if (SongData.nNowPlaying >= 0) {
				PlayClass.MusicPrepare(SongData.nNowPlaying, -1 * Pref.nRandomSeed, false);
			}
		}

		private void buttonNext_Click(object sender, RoutedEventArgs e) {
			if (SongData.nNowPlaying >= 0) {
				PlayClass.MusicPrepare(SongData.nNowPlaying, Pref.nRandomSeed, false);
			}
		}

		#region Music list drag & drop

		private void MainWindow_PreviewDragEnter(object sender, DragEventArgs e) {
			var dropPossible = e.Data != null && ((DataObject)e.Data).ContainsFileDropList();
			if (dropPossible) { gridDrop.Visibility = Visibility.Visible; }
		}

		private void MainWindow_PreviewDrop(object sender, DragEventArgs e) {
			gridDrop.Visibility = Visibility.Collapsed;
			if (!(e.Data is DataObject) || !((DataObject)e.Data).ContainsFileDropList()) { return; }

			Queue<string> Q = new Queue<string>();
			string strPath;
			bool isOK;
			List<SongData> listAdd = new List<SongData>();

			// BFS
			foreach (string filePath in ((DataObject)e.Data).GetFileDropList()) {
				if (Directory.Exists(filePath)) {
					Q.Enqueue(filePath);

					for (; ; ) {
						if (Q.Count == 0) { break; }
						strPath = Q.Dequeue();
						foreach (string subfile in Directory.GetDirectories(strPath)) {
							if (Directory.Exists(subfile)) { Q.Enqueue(subfile); }
						}

						foreach (string subfile in Directory.GetFiles(strPath)) {
							if (File.Exists(subfile)) {
								SongData sData = new SongData() { strFilePath = subfile };
								isOK = TagLibrary.InsertTagInDatabase(sData);

								if (isOK) {
									listAdd.Add(sData);
								}
							}
						}
					}
				} else if (File.Exists(filePath)) {
					SongData sData = new SongData() { strFilePath = filePath };
					isOK = TagLibrary.InsertTagInDatabase(sData);

					if (isOK) {
						listAdd.Add(sData);
					}
				}
			}

			FileAdd(listAdd);
			RefreshSongList(listAdd);
		}

		private void FileAdd(List<SongData> listAdd) {
			// Extract Sort Index
			foreach (SongData sData in listAdd) {
				sData.nID = SongData.nCount;

				int nHeaderIndex = FileIO.GetIndexerHeaderFrom(sData.strTitle); 

				sData.strSortTag = string.Format("{0:D4}{1}", nHeaderIndex, sData.strTitle);
				sData.nHeadIndex = FileIO.IndexUnique.IndexOf(FileIO.IndexValue[nHeaderIndex]);

				ListSong.Add(sData);
				SongData.DictSong.Add(SongData.nCount, sData);
				SongData.nCount++;

				TitleTree.AddToTree(sData.nID);

				Grid grid = CustomControl.GetListItemButton(sData, true);
				SongData.DictSong[sData.nID].gBase = grid;
				stackList.Children.Add(grid);

				((Button)grid.Children[3]).Click += SongListItem_Click;
				((Button)grid.Children[3]).MouseDoubleClick += SongListItem_DoubleClick;
			}
		}

		#endregion

		#region Indexer event

		private void gridIndexerRoot_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.RightButton == MouseButtonState.Pressed) {
				OneTimeCancel = true;
			}
			gridIndexerRoot.Visibility = Visibility.Collapsed;
		}

		private void buttonIndexer_Click(object sender, RoutedEventArgs e) {
			if (!Pref.isListVisible) { return; }
			if (gridIndexerRoot.Visibility == Visibility.Visible) {
				gridIndexerRoot.Visibility = Visibility.Collapsed;
			} else {
				gridIndexerRoot.Opacity = 0;
				gridIndexerRoot.Visibility = Visibility.Visible;
				gridIndexerRoot.BeginAnimation(Grid.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(120)));
			}
		}

		private void stackList_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			if (e.MiddleButton == MouseButtonState.Pressed) {
				buttonIndexer.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			}
		}

		#endregion

		#region Playing Control Event

		bool isPlayTimeMouseDown = false;
		private void gridPlayTime_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
			if (Pref.isPlaying == 0) { return; }
			double mouseMovingPixel = e.GetPosition(gridPlayTime).X;
			mouseMovingPixel = Math.Min(gridPlayTime.ActualWidth, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			try {
				PlayClass.mp.Position = new TimeSpan(0, 0, (int)((mouseMovingPixel / gridPlayTime.ActualWidth) * PlayClass.mp.NaturalDuration.TimeSpan.TotalSeconds));
				rectPlayTime.Width = mouseMovingPixel;
			} catch { }

			isPlayTimeMouseDown = true;
			gridPlayTime.CaptureMouse();
		}

		private void gridPlayTime_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			isPlayTimeMouseDown = false;
			gridPlayTime.ReleaseMouseCapture();
		}

		void gridPlayTime_PreviewMouseMove(object sender, MouseEventArgs e) {
			if (Pref.isPlaying == 0) { return; }
			if (!isPlayTimeMouseDown) { return; }
			double mouseMovingPixel = e.GetPosition(gridPlayTime).X;
			mouseMovingPixel = Math.Min(gridPlayTime.ActualWidth, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			try {
				PlayClass.mp.Position = new TimeSpan(0, 0, (int)((mouseMovingPixel / gridPlayTime.ActualWidth) * PlayClass.mp.NaturalDuration.TimeSpan.TotalSeconds));
				rectPlayTime.Width = mouseMovingPixel;
			} catch { }
		}
		#endregion

		#region Volume Control Event

		bool isVolumeMouseDown = false;
		private void gridVolume_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
			double mouseMovingPixel = e.GetPosition(gridVolume).X;
			mouseMovingPixel = Math.Min(50, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			PlayClass.mp.Volume = mouseMovingPixel / 50;
			rectVolume.Width = mouseMovingPixel;

			isVolumeMouseDown = true;
			gridVolume.CaptureMouse();
		}

		private void gridVolume_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			isVolumeMouseDown = false;
			gridVolume.ReleaseMouseCapture();
		}

		private void gridVolume_PreviewMouseMove(object sender, MouseEventArgs e) {
			if (!isVolumeMouseDown) { return; }
			double mouseMovingPixel = e.GetPosition(gridVolume).X;
			mouseMovingPixel = Math.Min(50, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			//textIndex.Text = mp.Volume.ToString();

			PlayClass.mp.Volume = mouseMovingPixel / 50;
			rectVolume.Width = mouseMovingPixel;
		}
		#endregion

		private void ToggleList() {
			Pref.isListVisible = !Pref.isListVisible;
			double nTowardHeight = Pref.isListVisible ? Pref.nListHeight : 192;
			if (!Pref.isListVisible) {
				gridIndexerRoot.Visibility = Visibility.Collapsed;
				this.MinHeight = 192;
				Pref.nListHeight = this.ActualHeight;
			} else { this.MaxHeight = 3000; }

			buttonHideList.Visibility = Pref.isListVisible ? Visibility.Visible : Visibility.Collapsed;
			buttonShowList.Visibility = Pref.isListVisible ? Visibility.Collapsed : Visibility.Visible;

			if (Pref.isPrefVisible && !Pref.isListVisible) {
				buttonPref.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			}

			Storyboard sb = new Storyboard();
			DoubleAnimation da = new DoubleAnimation(this.ActualHeight, nTowardHeight, TimeSpan.FromMilliseconds(300)) {
				BeginTime = TimeSpan.FromMilliseconds(100),
				EasingFunction = new PowerEase() {
					Power = 4, EasingMode = EasingMode.EaseInOut
				}
			};
			Storyboard.SetTarget(da, this);
			Storyboard.SetTargetProperty(da, new PropertyPath(Window.HeightProperty));
			sb.Children.Add(da);
			sb.Completed += delegate(object sender, EventArgs e) {
				if (Pref.isListVisible) {
					this.MinHeight = 540;
				} else {
					this.MaxHeight = 192;
				}
			};
			sb.Begin(this);
			FileIO.SavePreference();
		}

		private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			if (msg == Win32.WM_HOTKEY && Pref.isHotkeyOn) {
				if (wParam.ToString() == PPkey.ToString()) {
					TogglePlayingStatus();
				} else if (wParam.ToString() == Stopkey.ToString()) {
					StopPlayer();
				} else if (wParam.ToString() == Prevkey.ToString()) {
					if (SongData.nNowPlaying >= 0) {
						PlayClass.MusicPrepare(SongData.nNowPlaying, -1 * Pref.nRandomSeed, true);
					}
				} else if (wParam.ToString() == Nextkey.ToString()) {
					if (SongData.nNowPlaying >= 0) {
						PlayClass.MusicPrepare(SongData.nNowPlaying, Pref.nRandomSeed, true);
					}
				} else if (wParam.ToString() == Lyrkey.ToString()) {
					Pref.isLyricsVisible = !Pref.isLyricsVisible;
					buttonLyricsOn.Visibility = Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
					buttonLyricsOff.Visibility = !Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;

					LyricsWindow.ToggleLyrics(Pref.isLyricsVisible);
					LyrWindow.Checked = Pref.isLyricsVisible;
					FileIO.SavePreference();
				} else if (wParam.ToString() == SynPkey.ToString()) {
					if (Pref.isPlaying != 0) {
						LyricsWindow.ChangeOffset(-200);
					}
				} else if (wParam.ToString() == SynNkey.ToString()) {
					if (Pref.isPlaying != 0) {
						LyricsWindow.ChangeOffset(200);
					}
				} else if (wParam.ToString() == LaunchKey.ToString()) {
					ActivateWindow();
					if (!Pref.isListVisible) { ToggleList(); }
				}
				handled = true;
			}
			return IntPtr.Zero;
		}

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

		public bool ProcessCommandLineArgs(IList<string> args) {
			ActivateWindow();
			if (!Pref.isListVisible) { ToggleList(); }

			if (args == null || args.Count == 0) { return true; }

			return true;
		}

		public void ChangeThemeColor(Color color, bool isSave = true) {
			MainColor = color;
			grideffectShadow.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation(color, TimeSpan.FromMilliseconds(250)));
			gStop1.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation(color, TimeSpan.FromMilliseconds(250)));

			Application.Current.Resources["sColor"] = new SolidColorBrush(MainColor);
			Application.Current.Resources["cColor"] = Color.FromArgb(255, MainColor.R, MainColor.G, MainColor.B);
			CustomControl.sColor = MainBrush = FindResource("sColor") as SolidColorBrush;

			if (isSave) { FileIO.SavePreference(); }
		}

		private void imageAlbumart_MouseDown(object sender, MouseButtonEventArgs e) {
			if (SongData.nNowSelected < 0 || !SongData.DictSong.ContainsKey(SongData.nNowPlaying)) { return; }

			ChangeSelection(SongData.nNowPlaying);
			ScrollingList(SongData.DictSong[SongData.nNowPlaying].nPosition, 0);
		}

		private void ChangeSelection(int tag) {
			if (SongData.nNowSelected >= 0 && SongData.DictSong.ContainsKey(SongData.nNowSelected)) {
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).FontWeight = FontWeights.Normal;
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).Foreground = Brushes.Black;
			}
			SongData.nNowSelected = tag;

			((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).FontWeight = FontWeights.ExtraBold;
		}

		private void dtmOverlay_Tick(object sender, EventArgs e) {
			if (Pref.isPlaying <= 0) { return; }
			GridNowPlay.OpacityMask = ImgNowPlayArray[(++OpacityMaskIndex) % 3];
		}

		private void OptionColorBinder(TextBlock txt, bool b) {
			if (b) {
				txt.SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			} else { txt.Foreground = Brushes.LightGray; }
		}
	}
}
