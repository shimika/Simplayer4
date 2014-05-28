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

		#region Global variables

		public SolidColorBrush MainBrush;
		public System.Windows.Forms.NotifyIcon NI = new System.Windows.Forms.NotifyIcon();
		public System.Windows.Forms.ToolStripMenuItem LyrWindow = new System.Windows.Forms.ToolStripMenuItem("가사창 표시");
		System.Windows.Forms.ToolStripMenuItem MenuShutdown = new System.Windows.Forms.ToolStripMenuItem("닫기");
		private int PPkey, Stopkey, Prevkey, Nextkey, Lyrkey, SynPkey, SynNkey, LaunchKey;
		HwndSource HWndSource;
		public PreviewWindow PrevWindow;
		public ChangeNotification ChangeNotiWindow;
		public LyricsWindow LyricsWindow;
		bool OneTimeCancel = false;
		Color MainColor = Colors.SlateBlue;
		string Version = "ver 4.1.0";
		public Grid GridNowPlay = null;
		ImageBrush[] ImgNowPlayArray = new ImageBrush[3];
		int OpacityMaskIndex = 0;

		#endregion

		public MainWindow() {
			InitializeComponent();
		}

		private void Window_Loaded(object sender4, RoutedEventArgs e4) {
			CustomControl.sColor = MainBrush = FindResource("sColor") as SolidColorBrush;

			gridTitlebar.MouseLeftButtonDown += (o, e) => {
				try { DragMove(); } catch (Exception ex) {
					MessageBox.Show(ex.Message);
				}
			};
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

			PrevWindow = new PreviewWindow();
			PrevWindow.Show();

			ChangeNotiWindow = new ChangeNotification();
			ChangeNotiWindow.Show();

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

			dtm_Keydown.Tick += dtm_Keydown_Tick;

			InitPlayer();

			var iconHandle = Simplayer4.Properties.Resources.Music2.Handle;
			NI.Icon = System.Drawing.Icon.FromHandle(iconHandle);
			NI.Visible = true; NI.Text = "Simplayer4";
			NI.MouseDoubleClick +=
				delegate(object sender, System.Windows.Forms.MouseEventArgs e) {
					if (e.Button == System.Windows.Forms.MouseButtons.Left) {
						ActivateWindow();
					}
				};
			System.Windows.Forms.ContextMenuStrip ctxt = new System.Windows.Forms.ContextMenuStrip();
			this.Closing += delegate(object sender, CancelEventArgs e) {
				NI.Dispose(); Application.Current.Shutdown();
			};

			MenuShutdown.Click += delegate(object sender, EventArgs e) { Application.Current.Shutdown(); };
			LyrWindow.Click += delegate(object sender, EventArgs e) {
				Pref.isLyricsVisible = !Pref.isLyricsVisible;

				buttonLyricsOn.Visibility = Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
				buttonLyricsOff.Visibility = !Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;

				LyricsWindow.ToggleLyrics(Pref.isLyricsVisible);
				LyrWindow.Checked = Pref.isLyricsVisible;
				FileIO.SavePreference();
			};
			ctxt.Items.Add(LyrWindow);
			ctxt.Items.Add(MenuShutdown);
			NI.ContextMenuStrip = ctxt;

			rectVolume.Width = Pref.nVolume;
			PlayClass.mp.Volume = Pref.nVolume / 50;

			WindowInteropHelper wih = new WindowInteropHelper(this);
			HWndSource = HwndSource.FromHwnd(wih.Handle);
			HWndSource.AddHook(MainWindowProc);

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

		List<SongData> ListSong = null;
		private void InitPlayer() {
			textArtist.Text = Version;
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
			LyrWindow.Checked = Pref.isLyricsVisible;

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

			LyricsWindow = new LyricsWindow(Pref.isLyricsVisible);
			LyricsWindow.Show();

			ListSong = FileIO.ReadSongList();

			for (int i = 0; i < ListSong.Count; i++) {
				Grid grid = CustomControl.GetListItemButton(ListSong[i], false);
				SongData.DictSong[ListSong[i].nID].gBase = grid;
				stackList.Children.Add(grid);

				((Button)grid.Children[3]).Click += SongListItem_Click;
				((Button)grid.Children[3]).MouseDoubleClick += SongListItem_DoubleClick;
			}

			ActivateWindow();
			stackList.BeginAnimation(StackPanel.OpacityProperty,
				new DoubleAnimation(1, TimeSpan.FromMilliseconds(300)));

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

					ChangeThemeColor(Pref.cTheme);
				}
			};

			// 재생 중 이미지
			for (int i = 0; i < 3; i++) {
				ImgNowPlayArray[i] = new ImageBrush(CustomControl.rtSource(string.Format("iconPlaying{0}.png", i)));
			}

			GridNowPlay = new Grid() {
				Width = 24, Height = 24,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			GridNowPlay.SetResourceReference(Grid.BackgroundProperty, "sColor");
			GridNowPlay.OpacityMask = ImgNowPlayArray[0];

			DispatcherTimer dtmOverlay = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200), IsEnabled = true };
			dtmOverlay.Tick += dtmOverlay_Tick;
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
					if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
						try {
							PlayClass.mp.Position = new TimeSpan(0, 0, (int)PlayClass.mp.Position.TotalSeconds - 3);
						} catch { }
					} else {
						if (SongData.nNowPlaying >= 0) {
							PlayClass.MusicPrepare(SongData.nNowPlaying, -1 * Pref.nRandomSeed, false);
						}
					}
					return;
				case Key.Right:
					if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
						try {
							PlayClass.mp.Position = new TimeSpan(0, 0, (int)PlayClass.mp.Position.TotalSeconds + 3);
						} catch { }
					} else {
						if (SongData.nNowPlaying >= 0) {
							PlayClass.MusicPrepare(SongData.nNowPlaying, Pref.nRandomSeed, false);
						}
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
				nIndex = Math.Max(nIndex, 0);
				nIndex = Math.Min(nIndex, SongData.DictSong.Count - 1);

				ChangeSelection(PlayClass.nPositionArray[nIndex]);

				ScrollingList(nIndex, -1);
				return;
			}

			// Indexer shortcut

			int nKeyIndex = -1, nStart = 15, nEnd = 40;
			if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
				nStart = 1; nEnd = 14;
			} else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
				nStart = 41; nEnd = 50;
			}

			if (e.Key.ToString()[0] == 'D' && Char.IsNumber(e.Key.ToString(), e.Key.ToString().Length - 1)) { nKeyIndex = 0; }

			for (int i = nStart; i <= nEnd; i++) {
				if (IndexHeader[i].ToString() == e.Key.ToString()) {
					nKeyIndex = i;
				}
			}

			if (nKeyIndex >= 0) {
				if (!Pref.isListVisible) { ToggleList(); }

				// Alphabet
				if (nStart == 15 && nEnd == 40) {
					dtm_Keydown.Stop();
					dtm_Keydown.Start();

					int idx = TitleTree.GetPositionByHeader(IndexHeader[nKeyIndex]);

					if (idx >= 0) {
						ChangeSelection(idx);
						ScrollingList(SongData.DictSong[idx].nPosition, 0);
					}
				} else {
					IndexerShortCut(nKeyIndex);
				}
			}
		}

		// Indexer shortcut keydown timeout

		DispatcherTimer dtm_Keydown = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1), };
		private void dtm_Keydown_Tick(object sender, EventArgs e) {
			(sender as DispatcherTimer).Stop();
			TitleTree.LastPointer = 0;
		}

		string IndexHeader = "1RSEFAQTDWCZXVGABCDEFGHIJKLMNOPQRSTUVWXYZAKSTNHMYRW";
		double TowardOffset = -1;

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

			if (TowardOffset > 0) { nowOffset = TowardOffset; }
			DoubleAnimation da = new DoubleAnimation(nowOffset, newOffset, TimeSpan.FromMilliseconds(500)) {
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

		bool isDeleteProcessing = false;
		public void DeleteProcess(int nID) {
			if (nID < 0) { return; }
			if (isDeleteProcessing) { return; }
			if (!SongData.DictSong.ContainsKey(nID)) { return; }

			isDeleteProcessing = true;
			DispatcherTimer dtm = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
			dtm.Tick += delegate(object senderX, EventArgs eX) { isDeleteProcessing = false; ((DispatcherTimer)senderX).Stop(); };
			dtm.Start();

			try {
				((Grid)SongData.DictSong[nID].gBase.Children[4]).Children.Clear();
			} catch { }

			TitleTree.DeleteFromTree(nID);

			int delIndex = SongData.DictSong[nID].nPosition;
			stackList.Children.RemoveAt(delIndex);
			SongData.DictSong.Remove(nID);

			PlayClass.ShuffleList();
			FileIO.SaveSongList();

			if (nID == SongData.nNowSelected && SongData.DictSong.Count > 0) {
				SongData.nNowSelected = PlayClass.nPositionArray[Math.Min(SongData.DictSong.Count - 1, delIndex)];
				ChangeSelection(SongData.nNowSelected);
			}

			if (Pref.isSorted) { ListOrder.RefreshIndexer(); }
		}



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

			if (Pref.nTheme == 6) {
				ChangeThemeColor(Colors.Black, false);
			}

			if (SongData.nNowPlaying >= 0 && SongData.DictSong.ContainsKey(SongData.nNowPlaying)) {
				//((Grid)SongData.DictSong[SongData.nNowPlaying].gBase.Children[4]).Visibility = Visibility.Collapsed;
				try {
					((Grid)SongData.DictSong[SongData.nNowPlaying].gBase.Children[4]).Children.Clear();
				} catch { }
			}
			SongData.nNowPlaying = -1;

			PlayClass.mp.Stop();
		}

		private void SongListItem_Click(object sender, RoutedEventArgs e) {
			ChangeSelection((int)((Button)sender).Tag);

			if (SongData.nNowSelected == ReArrange.nPrevMovingIndex || !Pref.isOneClickPlaying) { return; }

			PlayClass.ShuffleList();
			PlayClass.MusicPrepare(SongData.nNowSelected, 0, false);
		}

		private void SongListItem_DoubleClick(object sender, MouseButtonEventArgs e) {
			if (Pref.isOneClickPlaying) { return; }

			PlayClass.ShuffleList();
			PlayClass.MusicPrepare(SongData.nNowSelected, 0, false);
		}

		private void RefreshSongList(List<SongData> listAdd) {
			PlayClass.ShuffleList();
			FileIO.SaveSongList();
			ShowMessage(string.Format("{0}개의 음악이 추가되었습니다.", listAdd.Count), 2);

			if (Pref.isAutoSort) {
				ListOrder.ListSort();
				int minIndex = 999999999;

				foreach (SongData sd in listAdd) {
					if (minIndex > sd.nPosition) {
						minIndex = sd.nPosition;
					}
				}

				if (minIndex != 999999999) {
					ScrollingList(minIndex, 0);
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
		
		string IndexerFormattedHeader = "1ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎ_ABCDEFGHIJKLMNOPQRSTUVWXYZ______あかさたなはまやらわ#";
		private void IndexerPreset() {
			int nCounter = 0;
			for (int i = 0; i < IndexerFormattedHeader.Length; i++) {
				if (IndexerFormattedHeader[i] == '_') { continue; }
				Button button = new Button() {
					Width = 36, Height = 36, Style = FindResource("FlatButton") as Style,
					HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
					Tag = nCounter++,
				};
				button.SetResourceReference(Button.BackgroundProperty, "sColor");

				TextBlock txt = new TextBlock() {
					Foreground = Brushes.White, Margin = new Thickness(4, 2, 0, 0), FontSize = 14,
					Text = IndexerFormattedHeader[i].ToString(),
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

			ChangeSelection((int)((Grid)stackList.Children[ListOrder.nIndexerPosition[nIndex]]).Tag);
			ScrollingList(ListOrder.nIndexerPosition[nIndex], 0);
		}
	}
}
