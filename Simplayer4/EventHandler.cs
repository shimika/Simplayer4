﻿using System;
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
using System.Windows.Media.Animation;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		[DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		const UInt32 SWP_NOSIZE = 0x0001;
		const UInt32 SWP_NOMOVE = 0x0002;

		static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		static void SendWpfWindowBack(Window window) {
			var hWnd = new WindowInteropHelper(window).Handle;
			SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
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
	}
}
