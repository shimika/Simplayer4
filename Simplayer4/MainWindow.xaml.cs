using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Simplayer4 {
	/// <summary>
	/// MainWindow.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class MainWindow : Window {
		public SolidColorBrush sColor;
		public System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
		public System.Windows.Forms.ToolStripMenuItem cLyrWindow = new System.Windows.Forms.ToolStripMenuItem("가사창 표시");
		System.Windows.Forms.ToolStripMenuItem cshutdown = new System.Windows.Forms.ToolStripMenuItem("닫기");
		private int PPkey, Stopkey, Prevkey, Nextkey, Lyrkey, SynPkey, SynNkey, LaunchKey;
		HwndSource hWndSource;
		public PreviewWindow pWindow;
		public ChangeNotification cWindow;
		public LyricsWindow lyrWindow;
		bool OneTimeCancel = false;


		public MainWindow() {
			InitializeComponent();

			CustomControl.sColor = sColor = FindResource("sColor") as SolidColorBrush;

			gridTitlebar.MouseLeftButtonDown += (o, e) => DragMove();
			buttonMinimize.Click += buttonMinimize_Click;
			buttonClose.Click += (o, e) => this.Close();
			this.Activated += (o, e) => grideffectShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.5, TimeSpan.FromMilliseconds(100)));
			this.Deactivated += (o, e) => grideffectShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.1, TimeSpan.FromMilliseconds(100)));
			this.SizeChanged += (o, e) => rectPlayTime.Width = rectTotalTime.ActualWidth * PlayClass.dPlayPerTotal;

			this.PreviewMouseMove += (o, e) => ReArrange.MouseMove(e.GetPosition(gridListArea));
			this.PreviewMouseUp += (o, e) => ReArrange.MouseUp();

			this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - 400;
			this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2 - 300;

			ListOrder.winMain = this;
			IndexerPreset();

			PlayClass.winMain = this;
			PlayClass.StartPlayer();

			CustomControl.winMain = this;
			ReArrange.winMain = this;

			pWindow = new PreviewWindow();
			pWindow.Show();

			cWindow = new ChangeNotification();
			cWindow.Show();

			this.ContextMenuOpening += MainWindow_ContextMenuOpening;
			this.ContextMenuClosing += (o, e) => { gridContextBlock.Visibility = Visibility.Collapsed; };
			gridContextBlock.MouseDown += (o, e) => { gridContextBlock.Visibility = Visibility.Collapsed; };

			// Pref button
			gridPrefBackground.MouseDown += (o, e) => buttonPref.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			buttonPref.Click += buttonPref_Click;

			PrefWindow.winMain = this;
			PrefWindow.PrefWindowPreset();

			buttonHideList.Click += (o, e) => ToggleList();
			buttonShowList.Click += (o, e) => ToggleList();

			// PlayTime Event

			gridPlayTime.PreviewMouseDown += gridPlayTime_PreviewMouseDown;
			gridPlayTime.PreviewMouseUp += gridPlayTime_PreviewMouseUp;
			gridPlayTime.PreviewMouseMove += gridPlayTime_PreviewMouseMove;

			// Volume Control Event
			gridVolume.PreviewMouseDown += gridVolume_PreviewMouseDown;
			gridVolume.PreviewMouseUp += gridVolume_PreviewMouseUp;
			gridVolume.PreviewMouseMove += gridVolume_PreviewMouseMove;

			// Controlbox Event
			buttonPrev.Click += buttonPrev_Click;
			buttonNext.Click += buttonNext_Click;
			buttonPlay.Click += (o, e) => TogglePlayingStatus();
			buttonPause.Click += (o, e) => TogglePlayingStatus();

			PreviewDragLeave += (o, e) => gridDrop.Visibility = Visibility.Collapsed;
			PreviewDragOver += (o, e) => e.Handled = true;
			PreviewDragEnter += MainWindow_PreviewDragEnter;
			PreviewDrop += MainWindow_PreviewDrop;

			// Indexer
			gridIndexerRoot.MouseDown += gridIndexerRoot_MouseDown;
			buttonIndexer.Click += buttonIndexer_Click;
			stackList.PreviewMouseDown += stackList_PreviewMouseDown;
			buttonIndexerSort.Click += (o, e) => ListOrder.ListSort();

			InitPlayer();
		}	
		



		private void TogglePlayingStatus() {
			switch (Pref.isPlaying) {
				case -1: PlayClass.ResumeMusic(); break;
				case 0: PlayClass.MusicPrepare(-1, 1, true); break;
				case 1: PlayClass.PauseMusic(); break;
			}
		}

		private void Window_KeyDown(object sender, KeyEventArgs e) {
			if (!Pref.isShowing) { return; }

			switch (e.Key) {
				case Key.Space:
					TogglePlayingStatus();
					return;
				case Key.Left:
					if (SongData.nNowPlaying >= 0) {
						PlayClass.MusicPrepare(SongData.nNowPlaying, -1 * Pref.nRandomSeed, false);
					}
					return;
				case Key.Right:
					if (SongData.nNowPlaying >= 0) {
						PlayClass.MusicPrepare(SongData.nNowPlaying, Pref.nRandomSeed, false);
					}
					return;
				case Key.OemQuestion:
					ToggleList();
					return;
				case Key.Back:
					buttonPref.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					return;
				case Key.Escape:
					buttonMinimize.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
					return;
			}

			if (Pref.isPrefVisible) { return; }

			switch (e.Key) {
				case Key.Enter:
					PlayClass.ShuffleList();
					PlayClass.MusicPrepare(SongData.nNowSelected, 0, false);
					return;
				case Key.Delete:
					DeleteProcess(SongData.nNowSelected);
					return;
			}

			int nIndex = SongData.DictSong.Count * -1, nPrevIndex = SongData.nNowSelected;
			if (SongData.DictSong.ContainsKey(SongData.nNowSelected)) {
				nIndex = SongData.DictSong[SongData.nNowSelected].nPosition;
			}

			bool isSelectionChange = false;
			if (!Pref.isPrefVisible && Pref.isListVisible) {
				isSelectionChange = true;
				switch (e.Key) {
					case Key.Up: nIndex--; break;
					case Key.Down: nIndex++; break;
					case Key.PageUp: nIndex -= (int)(gridListArea.ActualHeight / 40); break;
					case Key.PageDown: nIndex += (int)(gridListArea.ActualHeight / 40); break;
					case Key.Home: nIndex = 0; break;
					case Key.End: nIndex = SongData.DictSong.Count - 1; break;
					case Key.Enter: break;
					default: isSelectionChange = false; break;
				}
			}

			if (isSelectionChange) {
				if (SongData.DictSong.ContainsKey(nPrevIndex)) {
					((TextBlock)SongData.DictSong[nPrevIndex].gBase.Children[0]).Foreground = Brushes.Black;
					((TextBlock)SongData.DictSong[nPrevIndex].gBase.Children[0]).FontWeight = FontWeights.Normal;
				}

				nIndex = Math.Max(nIndex, 0);
				nIndex = Math.Min(nIndex, SongData.DictSong.Count - 1);

				SongData.nNowSelected = PlayClass.nPositionArray[nIndex];
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).FontWeight = FontWeights.Bold;

				ScrollingList(nIndex, -1);
				return;
			}


			// Indexer shortcut

			int nKeyIndex = -1, nStart = 15, nEnd = 40;
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
				nStart = 1; nEnd = 14;
			} else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) {
				nStart = 41; nEnd = 50;
			}

			if (e.Key.ToString()[0] == 'D' && Char.IsNumber(e.Key.ToString(), e.Key.ToString().Length - 1)) { nKeyIndex = 0; }
			for (int i = nStart; i <= nEnd; i++) {
				if (sCut[i].ToString() == e.Key.ToString()) {
					nKeyIndex = i;
				}
			}
			if (nKeyIndex >= 0) {
				if (!Pref.isListVisible) { ToggleList(); }
				IndexerShortCut(nKeyIndex);
			}
		}

		string sCut = "1RSEFAQTDWCZXVGABCDEFGHIJKLMNOPQRSTUVWXYZAKSTNHMYRW";

		private void ToggleList() {
			Pref.isListVisible = !Pref.isListVisible;
			double nTowardHeight = Pref.isListVisible ? Pref.nListHeight : 192;
			if (!Pref.isListVisible) {
				gridIndexerRoot.Visibility = Visibility.Collapsed;
				this.MinHeight = 192;
				Pref.nListHeight = this.ActualHeight;
			} else { this.MaxHeight = 3000;}

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

		double towardOffset = -1;
		private void ScrollingList(int itemIndex, int offset) {
			double nowOffset = scrollList.VerticalOffset, newOffset = 0;
			if (offset < 0) {
				newOffset = nowOffset;
				if (itemIndex * 40 < nowOffset) {
					newOffset = itemIndex * 40;
				}
				if ((itemIndex + 1) * 40 > nowOffset + gridListArea.ActualHeight - 40) {
					newOffset = nowOffset + (((itemIndex + 1) * 40) - (nowOffset + gridListArea.ActualHeight - 40));
				}
				if (nowOffset == newOffset) { return; }
			} else { newOffset = (itemIndex + offset) * 40; }
			double Duration = Math.Abs(newOffset - nowOffset);

			newOffset = Math.Min(newOffset, SongData.DictSong.Count * 40 - gridListArea.ActualHeight);
			Storyboard sb = new Storyboard();

			if (towardOffset > 0) { nowOffset = towardOffset; }
			DoubleAnimation da = new DoubleAnimation(nowOffset, newOffset, TimeSpan.FromMilliseconds(500)) {
				EasingFunction = new ExponentialEase() { Exponent = 5, EasingMode = EasingMode.EaseOut },
				BeginTime = TimeSpan.FromMilliseconds(0),
			};
			Storyboard.SetTarget(da, scrollList);
			Storyboard.SetTargetProperty(da, new PropertyPath(AniScrollViewer.CurrentVerticalOffsetProperty));

			sb.Children.Add(da);
			sb.Completed += delegate(object sender, EventArgs e) { towardOffset = -1; };
			sb.Begin(this);

			towardOffset = newOffset;
		}

		bool isDeleteProcessing = false;
		public void DeleteProcess(int nId) {
			if (nId < 0) { return; }
			if (isDeleteProcessing) { return; }
			if (!SongData.DictSong.ContainsKey(nId)) { return; }

			isDeleteProcessing = true;
			DispatcherTimer dtm = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
			dtm.Tick += delegate(object senderX, EventArgs eX) { isDeleteProcessing = false; ((DispatcherTimer)senderX).Stop(); };
			dtm.Start();

			int delIndex = SongData.DictSong[nId].nPosition;
			stackList.Children.RemoveAt(delIndex);
			SongData.DictSong.Remove(nId);

			PlayClass.ShuffleList();
			FileIO.SaveSongList();

			if (nId == SongData.nNowSelected && SongData.DictSong.Count > 0) {
				SongData.nNowSelected = PlayClass.nPositionArray[Math.Min(SongData.DictSong.Count - 1, delIndex)];
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			}

			if (Pref.isSorted) { ListOrder.RefreshIndexer(); }
		}


		List<SongData> listSong = null;
		private void InitPlayer() {
			FileIO.ReadPreference();

			if (Pref.nTheme < 6) {
				ChangeThemeColor(((SolidColorBrush)((Button)gridTheme.Children[Pref.nTheme]).Background).Color);
			} else if (Pref.nTheme == 6) {
				ChangeThemeColor(Colors.Black);
			} else if (Pref.nTheme == 7) {
				((Button)gridTheme.Children[7]).Background = new SolidColorBrush(Pref.cTheme);
				ChangeThemeColor(Pref.cTheme);
			}

			buttonLyricsOff.Visibility = !Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
			buttonLyricsOn.Visibility = Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
			cLyrWindow.Checked = Pref.isLyricsVisible;

			buttonLinear.Visibility = Pref.nRandomSeed == 1 ? Visibility.Visible : Visibility.Collapsed;
			buttonRandom.Visibility = Pref.nRandomSeed == 2 ? Visibility.Visible : Visibility.Collapsed;

			buttonPlayAll.Visibility = Pref.nPlayingLoopSeed == 1 ? Visibility.Visible : Visibility.Collapsed;
			buttonRepeat.Visibility = Pref.nPlayingLoopSeed == 0 ? Visibility.Visible : Visibility.Collapsed;

			buttonHideList.Visibility = Pref.isListVisible ? Visibility.Visible : Visibility.Collapsed;
			buttonShowList.Visibility = !Pref.isListVisible ? Visibility.Visible : Visibility.Collapsed;
			if (!Pref.isListVisible) { this.Height = this.MinHeight = this.MaxHeight = 192; }

			OptionColorBinder((TextBlock)buttonClickOne.Content, Pref.isOneClickPlaying);
			OptionColorBinder((TextBlock)buttonClickDouble.Content, !Pref.isOneClickPlaying);

			OptionColorBinder((TextBlock)buttonNotifyOn.Content, Pref.isNofifyOn);
			OptionColorBinder((TextBlock)buttonNotifyOff.Content, !Pref.isNofifyOn);

			OptionColorBinder((TextBlock)buttonAutoSortOn.Content, Pref.isAutoSort);
			OptionColorBinder((TextBlock)buttonAutoSortOff.Content, !Pref.isAutoSort);

			OptionColorBinder((TextBlock)buttonTray.Content, Pref.isTray);
			OptionColorBinder((TextBlock)buttonTaskbar.Content, !Pref.isTray);
			if (!Pref.isTray) { this.ShowInTaskbar = true; }

			OptionColorBinder((TextBlock)buttonHotkeyOn.Content, Pref.isHotkeyOn);
			OptionColorBinder((TextBlock)buttonHotkeyOff.Content, !Pref.isHotkeyOn);

			OptionColorBinder((TextBlock)buttonTopmostOn.Content, Pref.isTopMost);
			OptionColorBinder((TextBlock)buttonTopmostOff.Content, !Pref.isTopMost);
			this.Topmost = Pref.isTopMost;

			OptionColorBinder((TextBlock)buttonLyricsLeft.Content, !Pref.isLyricsRight);
			OptionColorBinder((TextBlock)buttonLyricsRight.Content, Pref.isLyricsRight);

			gridIndexer.Visibility = Pref.isSorted ? Visibility.Visible : Visibility.Collapsed;
			buttonIndexerSort.Visibility = !Pref.isSorted ? Visibility.Visible : Visibility.Collapsed;


			lyrWindow = new LyricsWindow(Pref.isLyricsVisible);
			lyrWindow.Show();


			listSong = FileIO.ReadSongList();

			for (int i = 0; i < listSong.Count; i++) {
				Grid grid = CustomControl.GetListItemButton(listSong[i], false);
				SongData.DictSong[listSong[i].nID].gBase = grid;
				stackList.Children.Add(grid);

				((Button)grid.Children[3]).Click += SongListItem_Click;
				((Button)grid.Children[3]).MouseDoubleClick += SongListItem_DoubleClick;
			}
			PlayClass.ShuffleList();

			if (Pref.isAutoSort) { ListOrder.ListSort(); }
			if (Pref.isSorted && !Pref.isAutoSort) { ListOrder.RefreshIndexer(); }

			int nThemeCount = 0;
			foreach (Button button in gridTheme.Children) {
				nThemeCount++;

				// 6개만 프리셋을 설정
				if (nThemeCount == 7) { break; }

				button.Click += (o, e) => {
					((Button)gridTheme.Children[7]).Background = Brushes.White;

					Pref.nTheme = Convert.ToInt32((string)((Button)o).Tag);
					SolidColorBrush sc = (SolidColorBrush)((Button)o).Background;
					ChangeThemeColor(sc.Color);
				};
			}

			// 앨범아트 이미지 컬러
			((Button)gridTheme.Children[6]).Click += (o, e) => {
				Pref.nTheme = 6;

				if (Pref.isPlaying == 0) {
					ChangeThemeColor(Colors.Black);
				} else {
					// 현재 앨범아트의 평균 색상을 가져와서 변경
					Color c = TagLibrary.CalculateAverageColor((BitmapSource)imageAlbumart.Source);
					ChangeThemeColor(c);
				}
			};

			// 커스텀 컬러
			((Button)gridTheme.Children[7]).Click += (o, e) => {
				System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog() {
					AllowFullOpen = true,
				};
				if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
					Pref.nTheme = 7;
					Pref.cTheme = Color.FromRgb(colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);

					((Button)gridTheme.Children[7]).Background = new SolidColorBrush(Pref.cTheme);
					//imageCustomTheme.Visibility = Visibility.Collapsed;

					ChangeThemeColor(Pref.cTheme);
					//MessageBox.Show(colorDialog.Color.R + " : " + colorDialog.Color.G + " : " + colorDialog.Color.B);
				}
			};
		}

		private void OptionColorBinder(TextBlock txt, bool b) {
			if (b) {
				txt.SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			} else { txt.Foreground = Brushes.LightGray; }
		}

		private void Window_Loaded(object sender4, RoutedEventArgs e4) {
			var iconHandle = Simplayer4.Properties.Resources.Music2.Handle;
			ni.Icon = System.Drawing.Icon.FromHandle(iconHandle);
			ni.Visible = true; ni.Text = "Simplayer4";
			ni.MouseDoubleClick +=
				delegate(object sender, System.Windows.Forms.MouseEventArgs e) {
					if (e.Button == System.Windows.Forms.MouseButtons.Left) {
						ActivateWindow();
					}
				};
			System.Windows.Forms.ContextMenuStrip ctxt = new System.Windows.Forms.ContextMenuStrip();
			this.Closing += delegate(object sender, CancelEventArgs e) {
				ni.Dispose(); Application.Current.Shutdown();
			};

			cshutdown.Click += delegate(object sender, EventArgs e) { Application.Current.Shutdown(); };
			cLyrWindow.Click += delegate(object sender, EventArgs e) {
				Pref.isLyricsVisible = !Pref.isLyricsVisible;

				buttonLyricsOn.Visibility = Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
				buttonLyricsOff.Visibility = !Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;

				lyrWindow.ToggleLyrics(Pref.isLyricsVisible);
				cLyrWindow.Checked = Pref.isLyricsVisible;
				FileIO.SavePreference();
			};
			ctxt.Items.Add(cLyrWindow);
			ctxt.Items.Add(cshutdown);
			ni.ContextMenuStrip = ctxt;

			rectVolume.Width = Pref.nVolume;
			PlayClass.mp.Volume = Pref.nVolume / 50;

			WindowInteropHelper wih = new WindowInteropHelper(this);
			hWndSource = HwndSource.FromHwnd(wih.Handle);
			hWndSource.AddHook(MainWindowProc);

			PPkey = Win32.GlobalAddAtom("ButtonPP");
			Stopkey = Win32.GlobalAddAtom("ButtonStop");
			Prevkey = Win32.GlobalAddAtom("ButtonPrev");
			Nextkey = Win32.GlobalAddAtom("ButtonNext");
			Lyrkey = Win32.GlobalAddAtom("LyricsKey");
			SynPkey = Win32.GlobalAddAtom("SyncPrevKey");
			SynNkey = Win32.GlobalAddAtom("SyncNextKey");
			LaunchKey = Win32.GlobalAddAtom("ListShowHide");
			Win32.RegisterHotKey(wih.Handle, PPkey, 3, Win32.VK_DOWN);
			Win32.RegisterHotKey(wih.Handle, Stopkey, 3, Win32.VK_UP);
			Win32.RegisterHotKey(wih.Handle, Prevkey, 3, Win32.VK_LEFT);
			Win32.RegisterHotKey(wih.Handle, Nextkey, 3, Win32.VK_RIGHT);
			Win32.RegisterHotKey(wih.Handle, Lyrkey, 3, Win32.VK_KEY_D);
			Win32.RegisterHotKey(wih.Handle, SynPkey, 3, Win32.VK_OEM_COMMA);
			Win32.RegisterHotKey(wih.Handle, SynNkey, 3, Win32.VK_OEM_PERIOD);
			Win32.RegisterHotKey(wih.Handle, LaunchKey, 8, Win32.VK_KEY_W);

			FileIO.dtmSave.Start();
		}

		public void StopPlayer() {
			textPlayTime.Text = "00:00 / 00:00";
			buttonPlay.Visibility = Visibility.Visible;
			buttonPause.Visibility = Visibility.Collapsed;
			rectPlayTime.Width = Pref.isPlaying = 0;

			textTitle.Text = "Simplayer4";
			textArtist.Text = "ver.4"; textAlbum.Text = "";
			imageAlbumart.Source = TagLibrary.rtSource("noImage.png");

			lyrWindow.lS.Text = "";
			lyrWindow.lO.Text = "Offset:0ms";
			lyrWindow.lT.Text = "00:00 / 00:00";
			lyrWindow.ChangeLabels("Simplayer4", "", "");

			if (Pref.nTheme == 6) {
				ChangeThemeColor(Colors.Black, false);
			}

			if (SongData.nNowPlaying >= 0 && SongData.DictSong.ContainsKey(SongData.nNowPlaying)) {
				((Grid)SongData.DictSong[SongData.nNowPlaying].gBase.Children[4]).Visibility = Visibility.Collapsed;
			}

			PlayClass.mp.Stop();
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

					lyrWindow.ToggleLyrics(Pref.isLyricsVisible);
					cLyrWindow.Checked = Pref.isLyricsVisible;
					FileIO.SavePreference();
				} else if (wParam.ToString() == SynPkey.ToString()) {
					if (Pref.isPlaying != 0) {
						lyrWindow.ChangeOffset(-200);
					}
				} else if (wParam.ToString() == SynNkey.ToString()) {
					if (Pref.isPlaying != 0) {
						lyrWindow.ChangeOffset(200);
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

		private void SongListItem_Click(object sender, RoutedEventArgs e) {
			if (SongData.nNowSelected >= 0 && SongData.DictSong.ContainsKey(SongData.nNowSelected)) {
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).FontWeight = FontWeights.Normal;
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).Foreground = Brushes.Black;
			}
			SongData.nNowSelected = (int)((Button)sender).Tag;

			((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).FontWeight = FontWeights.ExtraBold;

			if (SongData.nNowSelected == ReArrange.nPrevMovingIndex) { return; }

			if (!Pref.isOneClickPlaying) { return; }

			PlayClass.ShuffleList();
			PlayClass.MusicPrepare(SongData.nNowSelected, 0, false);
		}

		private void SongListItem_DoubleClick(object sender, MouseButtonEventArgs e) {
			if (Pref.isOneClickPlaying) { return; }

			PlayClass.ShuffleList();
			PlayClass.MusicPrepare(SongData.nNowSelected, 0, false);
		}

		private void FileAdd(List<SongData> listAdd) {			
			// Extract Sort Index
			foreach (SongData sData in listAdd) {
				char cHead = FileIO.HangulDevide(sData.strTitle.ToUpper())[0];
				int idx = FileIO.strIndexCaption.IndexOf(cHead);
				if (idx < 0) { idx += FileIO.strIndexCaption.Length; }

				sData.strSortTag = string.Format("{0:D3}{1}", idx, sData.strTitle);
				sData.nHeadIndex = FileIO.strIndexUnique.IndexOf(FileIO.strIndexValue[idx]);
				sData.nID = SongData.nCount;

				listSong.Add(sData);
				SongData.DictSong.Add(SongData.nCount, sData);
				SongData.nCount++;

				Grid grid = CustomControl.GetListItemButton(sData, true);
				SongData.DictSong[sData.nID].gBase = grid;
				stackList.Children.Add(grid);

				((Button)grid.Children[3]).Click += SongListItem_Click;
				((Button)grid.Children[3]).MouseDoubleClick += SongListItem_DoubleClick;
			}
		}

		private void RefreshSongList(List<SongData> listAdd) {
			PlayClass.ShuffleList();
			FileIO.SaveSongList();
			//ShowMessage(string.Format("{0} song{1} ha{2} been added", listAdd.Count, listAdd.Count <= 1 ? "" : "s", listAdd.Count <= 1 ? "s" : "ve"), 2);
			ShowMessage(string.Format("{0}개의 음악이 추가되었습니다.", listAdd.Count), 2);

			if (Pref.isAutoSort) {
				ListOrder.ListSort();
				if (listAdd.Count > 0) {
					ScrollingList(listAdd[0].nPosition, 0);
				}
			} else {
				if (listAdd.Count > 0) {
					Pref.isSorted = false;
					gridIndexer.Visibility = Visibility.Collapsed;
					buttonIndexerSort.Visibility = Visibility.Visible;
					FileIO.SavePreference();

					if (listAdd.Count > 0) {
						ScrollingList(stackList.Children.Count - 1, -1);
					}
				}
			}
		}

		private void ShowMessage(string message, double dTimeout) {
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

		string strIndexer = "1ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎ_ABCDEFGHIJKLMNOPQRSTUVWXYZ______あかさたなはまやらわ#";
		private void IndexerPreset() {
			int nCounter = 0;
			for (int i = 0; i < strIndexer.Length; i++) {
				if (strIndexer[i] == '_') { continue; }
				Button button = new Button() {
					Width = 36, Height = 36, Style = FindResource("FlatButton") as Style,
					HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
					Tag = nCounter++,
				};
				button.SetResourceReference(Button.BackgroundProperty, "sColor");

				TextBlock txt = new TextBlock() {
					Foreground = Brushes.White, Margin = new Thickness(4, 2, 0, 0), FontSize = 14,
					Text = strIndexer[i].ToString(),
					TextAlignment = TextAlignment.Left, VerticalAlignment = VerticalAlignment.Top,
				};

				button.Content = txt;
				button.Click += (o, e) => {
					int nIndex = (int)((Button)o).Tag;
					IndexerShortCut(nIndex);
				};

				Grid.SetRow(button, i / 8);
				Grid.SetColumn(button, i % 8);
				gridIndexer.Children.Add(button);
			}
		}

		private void IndexerShortCut(int nIndex) {
			if (nIndex < 0) { return; }
			//textTemp.Text = ListOrder.nIndexerPosition[nIndex].ToString();
			gridIndexerRoot.Visibility = Visibility.Collapsed;

			if (!Pref.isSorted) { return; }
			if (ListOrder.nIndexerPosition[nIndex] < 0) { return; }

			if (SongData.DictSong.ContainsKey(SongData.nNowSelected)) {
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).Foreground = Brushes.Black;
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).FontWeight = FontWeights.Normal;
			}
			SongData.nNowSelected = (int)((Grid)stackList.Children[ListOrder.nIndexerPosition[nIndex]]).Tag;
			((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).FontWeight = FontWeights.Bold;

			ScrollingList(ListOrder.nIndexerPosition[nIndex], 0);
		}

		public bool ProcessCommandLineArgs(IList<string> args) {
			ActivateWindow();
			if (!Pref.isListVisible) { ToggleList(); }

			if (args == null || args.Count == 0) { return true; }

			return true;
		}

		Color mainColor = Colors.SlateBlue;
		public void ChangeThemeColor(Color color, bool isSave = true) {
			mainColor = color;
			grideffectShadow.BeginAnimation(DropShadowEffect.ColorProperty, new ColorAnimation(color, TimeSpan.FromMilliseconds(250)));
			gStop1.BeginAnimation(GradientStop.ColorProperty, new ColorAnimation(color, TimeSpan.FromMilliseconds(250)));

			Application.Current.Resources["sColor"] = new SolidColorBrush(mainColor);
			Application.Current.Resources["cColor"] = Color.FromArgb(255, mainColor.R, mainColor.G, mainColor.B);
			CustomControl.sColor = sColor = FindResource("sColor") as SolidColorBrush;

			if (isSave) { FileIO.SavePreference(); }
		}

		private void imageAlbumart_MouseDown(object sender, MouseButtonEventArgs e) {
			if (SongData.nNowSelected < 0 || !SongData.DictSong.ContainsKey(SongData.nNowPlaying)) { return; }


			if (SongData.nNowSelected >= 0 && SongData.DictSong.ContainsKey(SongData.nNowSelected)) {
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).FontWeight = FontWeights.Normal;
				((TextBlock)SongData.DictSong[SongData.nNowSelected].gBase.Children[0]).Foreground = Brushes.Black;
			}
			SongData.nNowSelected = SongData.nNowPlaying;

			((TextBlock)SongData.DictSong[SongData.nNowPlaying].gBase.Children[0]).SetResourceReference(TextBlock.ForegroundProperty, "sColor");
			((TextBlock)SongData.DictSong[SongData.nNowPlaying].gBase.Children[0]).FontWeight = FontWeights.ExtraBold;

			ScrollingList(SongData.DictSong[SongData.nNowPlaying].nPosition, 0);
		}
	}
}
