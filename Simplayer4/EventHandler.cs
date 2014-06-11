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
using System.Windows.Threading;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		private void SetEventHandlers() {
			// Global window
			Activated += MainWindow_Activated;
			Deactivated += MainWindow_Deactivated;
			SizeChanged += MainWindow_SizeChanged;
			Closing += MainWindow_Closing;

			// Titlebar movement, buttons
			gridTitlebar.MouseLeftButtonDown += gridTitlebar_MouseLeftButtonDown;
			buttonMinimize.Click += buttonMinimize_Click;
			buttonClose.Click += buttonClose_Click;

			// Preference
			buttonPref.Click += buttonPref_Click;
			gridPrefBackground.MouseDown += gridPrefBackground_MouseDown;

			// Context menu
			ContextMenuOpening += MainWindow_ContextMenuOpening;
			ContextMenuClosing += MainWindow_ContextMenuClosing;
			gridContextBlock.MouseDown += gridContextBlock_MouseDown;

			// Rearrange item
			PreviewMouseMove += MainWindow_PreviewMouseMove;
			PreviewMouseUp += MainWindow_PreviewMouseUp;

			// PlayTime
			gridPlayStatus.PreviewMouseDown += gridPlayStatus_PreviewMouseDown;
			gridPlayStatus.PreviewMouseUp += gridPlayStatus_PreviewMouseUp;
			gridPlayStatus.PreviewMouseMove += gridPlayStatus_PreviewMouseMove;

			// Volume Control
			gridVolume.PreviewMouseDown += gridVolume_PreviewMouseDown;
			gridVolume.PreviewMouseUp += gridVolume_PreviewMouseUp;
			gridVolume.PreviewMouseMove += gridVolume_PreviewMouseMove;

			// Controlbox event
			buttonPrev.Click += buttonPrev_Click;
			buttonNext.Click += buttonNext_Click;
			buttonPlay.Click += buttonPlayPause_Click;
			buttonPause.Click += buttonPlayPause_Click;

			// Indexer
			gridIndexerRoot.MouseDown += gridIndexerRoot_MouseDown;
			buttonIndexer.Click += buttonIndexer_Click;
			stackList.PreviewMouseDown += stackList_PreviewMouseDown;
			buttonIndexerSort.Click += buttonIndexerSort_Click;

			// List show + hide
			buttonHideList.Click += ListToggle_Click;
			buttonShowList.Click += ListToggle_Click;

			// File drag + drop event
			PreviewDragLeave += MainWindow_PreviewDragLeave;
			PreviewDragOver += MainWindow_PreviewDragOver;
			PreviewDragEnter += MainWindow_PreviewDragEnter;
			PreviewDrop += MainWindow_PreviewDrop;

			// Key Timeout, Playing icon
			Timer_Keydown.Tick += KeydownTimer_Tick;
			PlayingIconTimer.Tick += PlayingIconTimer_Tick;

			// Playing delay timer
			TimerDelay.Tick += TimerDelay_Tick;

			// Media Player events
			MusicPlayer.MediaEnded += MusicPlayer_MediaEnded;
			MusicPlayer.MediaFailed += MusicPlayer_MediaFailed;
		}

		// Global window event
		private void MainWindow_Activated(object sender, EventArgs e) {
			grideffectShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.5, TimeSpan.FromMilliseconds(100)));
		}
		private void MainWindow_Deactivated(object sender, EventArgs e) {
			grideffectShadow.BeginAnimation(DropShadowEffect.OpacityProperty, new DoubleAnimation(0.1, TimeSpan.FromMilliseconds(100)));
		}
		private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e) {
			rectPlayTime.Width = rectTotalTime.ActualWidth * PlayPerTotal;
		}
		private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			TrayNotify.Dispose();
			System.Windows.Application.Current.Shutdown();
		}

		// Titlebar movement and event
		private void gridTitlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
			try { DragMove(); } catch (Exception ex) {
				//MessageBox.Show(ex.Message);
			}
		}
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
		private void buttonClose_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}

		// Preference events
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
		private void gridPrefBackground_MouseDown(object sender, MouseButtonEventArgs e) {
			buttonPref.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
		}

		// Context menu event
		private void MainWindow_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
			if (ContextBlock || isMouseDown) {
				ContextBlock = false;
				e.Handled = true;
			} else {
				gridContextBlock.Visibility = Visibility.Visible;
			}
		}
		private void MainWindow_ContextMenuClosing(object sender, ContextMenuEventArgs e) {
			gridContextBlock.Visibility = Visibility.Collapsed;
		}
		private void gridContextBlock_MouseDown(object sender, MouseButtonEventArgs e) {
			gridContextBlock.Visibility = Visibility.Collapsed;
		}

		// Rearrange item event
		private void MainWindow_PreviewMouseMove(object sender, MouseEventArgs e) {
			MouseMove(e.GetPosition(gridListArea));
		}
		private void MainWindow_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			MouseReleaseUp();
		}

		// Now playing status bar
		bool isPlayTimeMouseDown = false;
		private void gridPlayStatus_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
			if (Pref.isPlaying == 0) { return; }
			double mouseMovingPixel = e.GetPosition(gridPlayStatus).X;
			mouseMovingPixel = Math.Min(gridPlayStatus.ActualWidth, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			try {
				MusicPlayer.Position = new TimeSpan(0, 0, (int)((mouseMovingPixel / gridPlayStatus.ActualWidth) * MusicPlayer.NaturalDuration.TimeSpan.TotalSeconds));
				rectPlayTime.Width = mouseMovingPixel;
			} catch { }

			isPlayTimeMouseDown = true;
			gridPlayStatus.CaptureMouse();
		}
		private void gridPlayStatus_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
			isPlayTimeMouseDown = false;
			gridPlayStatus.ReleaseMouseCapture();
		}
		private void gridPlayStatus_PreviewMouseMove(object sender, MouseEventArgs e) {
			if (Pref.isPlaying == 0) { return; }
			if (!isPlayTimeMouseDown) { return; }
			double mouseMovingPixel = e.GetPosition(gridPlayStatus).X;
			mouseMovingPixel = Math.Min(gridPlayStatus.ActualWidth, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			try {
				MusicPlayer.Position = new TimeSpan(0, 0, (int)((mouseMovingPixel / gridPlayStatus.ActualWidth) * MusicPlayer.NaturalDuration.TimeSpan.TotalSeconds));
				rectPlayTime.Width = mouseMovingPixel;
			} catch { }
		}

		// Volume control
		bool isVolumeMouseDown = false;
		private void gridVolume_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
			e.Handled = true;
			double mouseMovingPixel = e.GetPosition(gridVolume).X;
			mouseMovingPixel = Math.Min(50, mouseMovingPixel);
			mouseMovingPixel = Math.Max(0, mouseMovingPixel);

			MusicPlayer.Volume = mouseMovingPixel / 50;
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

			MusicPlayer.Volume = mouseMovingPixel / 50;
			rectVolume.Width = mouseMovingPixel;
		}

		// Playing control box
		private void buttonPrev_Click(object sender, RoutedEventArgs e) {
			if (SongData.NowPlaying >= 0) {
				MusicPrepare(SongData.NowPlaying, -1 * Pref.RandomSeed, false);
			}
		}
		private void buttonNext_Click(object sender, RoutedEventArgs e) {
			if (SongData.NowPlaying >= 0) {
				MusicPrepare(SongData.NowPlaying, Pref.RandomSeed, false);
			}
		}
		private void buttonPlayPause_Click(object sender, RoutedEventArgs e) {
			TogglePlayingStatus();
		}
		private void TogglePlayingStatus() {
			if (SongData.DictSong.Count == 0 && Pref.isPlaying == 0) { return; }
			switch (Pref.isPlaying) {
				case -1: ResumeMusic(); break;
				case 0: MusicPrepare(-1, 1, true); break;
				case 1: PauseMusic(); break;
			}
		}

		// Indexer
		bool ContextBlock = false;
		private void gridIndexerRoot_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.RightButton == MouseButtonState.Pressed) {
				ContextBlock = true;
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
		private void buttonIndexerSort_Click(object sender, RoutedEventArgs e) {
			SortList();

			ShuffleList();
			SaveSongList();
			SavePreference();

		}

		// List show + hide
		private void ListToggle_Click(object sender, RoutedEventArgs e) {
			ToggleList();
		}
		private void ToggleList() {
			Pref.isListVisible = !Pref.isListVisible;
			double nTowardHeight = Pref.isListVisible ? Pref.ListHeight : 192;
			if (!Pref.isListVisible) {
				gridIndexerRoot.Visibility = Visibility.Collapsed;
				this.MinHeight = 192;
				Pref.ListHeight = this.ActualHeight;
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
			SavePreference();
		}

		// File drag + drop event
		private void MainWindow_PreviewDragLeave(object sender, DragEventArgs e) {
			gridDrop.Visibility = Visibility.Collapsed;
		}
		private void MainWindow_PreviewDragOver(object sender, DragEventArgs e) {
			e.Handled = true;
		}
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
								SongData sData = new SongData() { FilePath = subfile };
								isOK = TagLibrary.InsertTagInDatabase(ref sData);

								if (isOK) {
									listAdd.Add(sData);
								}
							}
						}
					}
				} else if (File.Exists(filePath)) {
					SongData sData = new SongData() { FilePath = filePath };
					isOK = TagLibrary.InsertTagInDatabase(ref sData);

					if (isOK) {
						listAdd.Add(sData);
					}
				}
			}

			AddToLibrary(listAdd);
		}

		// Key timeout
		DispatcherTimer Timer_Keydown = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1), };
		private void KeydownTimer_Tick(object sender, EventArgs e) {
			(sender as DispatcherTimer).Stop();
			TitleTree.ClearSearchTag();
		}

		// Now playing animated icon
		DispatcherTimer PlayingIconTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200), IsEnabled = true };
		int OpacityMaskIndex = 0;
		private void PlayingIconTimer_Tick(object sender, EventArgs e) {
			if (Pref.isPlaying <= 0) { return; }
			GridNowPlay.OpacityMask = ImgNowPlayArray[(++OpacityMaskIndex) % 3];
		}

		// Albumart mousedown
		private void imageAlbumart_MouseDown(object sender, MouseButtonEventArgs e) {
			if (!SongData.DictSong.ContainsKey(SongData.NowPlaying)) { return; }

			ChangeSelection(SongData.NowPlaying);
			ScrollingList(SongData.DictSong[SongData.NowPlaying].Position, 1);
		}

		// Song item click
		private void SongListItem_Click(object sender, RoutedEventArgs e) {
			ChangeSelection((int)((Button)sender).Tag);

			if (SongData.NowSelected == nPrevMovingIndex || !Pref.isOneClickPlaying) { return; }

			ShuffleList();
			MusicPrepare(SongData.NowSelected, 0, false);
		}
		private void SongListItem_DoubleClick(object sender, MouseButtonEventArgs e) {
			if (Pref.isOneClickPlaying) { return; }

			ShuffleList();
			MusicPrepare(SongData.NowSelected, 0, false);
		}

		// Application keydown
		private void Window_KeyDown(object sender, KeyEventArgs e) {
			if (!Pref.isShowing) { return; }

			switch (e.Key) {
				case Key.Space:
					TogglePlayingStatus();
					return;
				case Key.Left:
					if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
						try {
							MusicPlayer.Position = new TimeSpan(0, 0, (int)MusicPlayer.Position.TotalSeconds - 3);
						} catch { }
					} else {
						if (SongData.NowPlaying >= 0) {
							MusicPrepare(SongData.NowPlaying, -1 * Pref.RandomSeed, false);
						}
					}
					return;
				case Key.Right:
					if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
						try {
							MusicPlayer.Position = new TimeSpan(0, 0, (int)MusicPlayer.Position.TotalSeconds + 3);
						} catch { }
					} else {
						if (SongData.NowPlaying >= 0) {
							MusicPrepare(SongData.NowPlaying, Pref.RandomSeed, false);
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
					ShuffleList();
					MusicPrepare(SongData.NowSelected, 0, false);
					return;
				case Key.Delete:
					FileDelete(SongData.NowSelected);
					return;
			}

			int nIndex = SongData.DictSong.Count * -1, nPrevIndex = SongData.NowSelected;
			if (SongData.DictSong.ContainsKey(SongData.NowSelected)) {
				nIndex = SongData.DictSong[SongData.NowSelected].Position;
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

				ChangeSelection(PositionArray[nIndex]);

				ScrollingList(nIndex, -1);
				return;
			}

			// Indexer shortcut

			if (!Pref.isSorted) { return; }

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
					Timer_Keydown.Stop();
					Timer_Keydown.Start();

					int idx = TitleTree.GetPositionByHeader(IndexHeader[nKeyIndex]);

					//ShowMessage(TitleTree.SearchTag + " : " + (idx >= 0 ? SongData.DictSong[idx].Title : "-"));

					if (idx >= 0) {
						ChangeSelection(idx);
						ScrollingList(SongData.DictSong[idx].Position, 0);
					}

				} else {
					LaunchIndexerKey(nKeyIndex);
				}
			}
		}

		// Playing delay timer
		private void TimerDelay_Tick(object sender, EventArgs e) {
			(sender as DispatcherTimer).Stop();
			isDelay = false;
		}

		// Media Player events
		private void MusicPlayer_MediaEnded(object sender, EventArgs e) {
			MusicPrepare(SongData.NowPlaying, Pref.PlayingLoopSeed * Pref.RandomSeed, false);
		}
		private void MusicPlayer_MediaFailed(object sender, ExceptionEventArgs e) {
			MusicPrepare(SongData.NowPlaying, Pref.PlayingLoopSeed * PlayingDirection, false, true);
		}
	}
}
