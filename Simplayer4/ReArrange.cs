using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Simplayer4 {
	public class ReArrange {
		public static MainWindow winMain;

		public static int nFromIndex, nToIndex, nMouseDownID, nMouseMovingID;
		public static bool isMoving, isMouseDown;
		public static Point pointMouseDown, pointMouseMove;

		public static void MouseDown(int nId, Point pPoint) {
			nPrevMovingIndex = -1;

			isMouseDown = true; isMoving = false;
			nMouseDownID = nId; nToIndex = -1;
			pointMouseDown = pPoint;
			nFromIndex = SongData.DictSong[nId].nPosition;
			winMain.textNowMoving.Text = SongData.DictSong[nId].strTitle;
		}

		public static void MouseMove(Point pPoint) {
			if (!isMouseDown || nMouseDownID < 0) { return; }
			pointMouseMove = pPoint;
			
			if (Math.Max(Math.Abs(pointMouseDown.X - pointMouseMove.X), Math.Abs(pointMouseDown.Y - pointMouseMove.Y)) >= 10 && !isMoving) {
				nMouseMovingID = nMouseDownID;
				isMoving = true;

				winMain.gridMoveStatus.Visibility = Visibility.Visible;
			}

			if (!isMoving) { return; }
			CalculatePoint();
		}

		private static void CalculatePoint() {
			if (!isMouseDown || nMouseDownID < 0 || !isMoving) { return; }

			if (pointMouseMove.X < 0 || pointMouseMove.X > winMain.gridListArea.ActualWidth) {
				nToIndex = -1;
				winMain.gridMoveStatus.Visibility = Visibility.Collapsed;
				return;
			} else {
				winMain.gridMoveStatus.Visibility = Visibility.Visible;
			}

			winMain.gridNowMoving.Margin = new Thickness(pointMouseMove.X - 100, pointMouseMove.Y - 20, 0, 0);

			if (pointMouseMove.Y < -10) {
				nToIndex = -1;
				winMain.rectMovePosition.Visibility = Visibility.Collapsed;

				if (isCornerScrollingDelay) { return; }
				isCornerScrollingDelay = true;
				DelayTimer(250, "isCornerScrollingDelay");

				winMain.scrollList.BeginAnimation(AniScrollViewer.CurrentVerticalOffsetProperty,
					new DoubleAnimation(winMain.scrollList.VerticalOffset, Math.Max(winMain.scrollList.VerticalOffset - 150, 0), TimeSpan.FromMilliseconds(200)) {
						EasingFunction = new ExponentialEase() { Exponent = 1, EasingMode = EasingMode.EaseInOut },
					});
			} else if (pointMouseMove.Y > winMain.gridListArea.ActualHeight + 10) {
				nToIndex = -1;
				winMain.rectMovePosition.Visibility = Visibility.Collapsed;

				if (isCornerScrollingDelay) { return; }
				isCornerScrollingDelay = true;
				DelayTimer(250, "isCornerScrollingDelay");

				winMain.scrollList.BeginAnimation(AniScrollViewer.CurrentVerticalOffsetProperty,
							new DoubleAnimation(winMain.scrollList.VerticalOffset, Math.Min(winMain.scrollList.VerticalOffset + 150, winMain.scrollList.ScrollableHeight), TimeSpan.FromMilliseconds(200)) {
								EasingFunction = new ExponentialEase() { Exponent = 1, EasingMode = EasingMode.EaseInOut },
							});
			} else {
				winMain.rectMovePosition.Visibility = Visibility.Visible;
				double pointAbsolute = winMain.scrollList.VerticalOffset + pointMouseMove.Y;
				int nHoverIndex = ((int)pointAbsolute) / 40;

				nHoverIndex = Math.Max(0, nHoverIndex);
				nHoverIndex = Math.Min(SongData.DictSong.Count - 1, nHoverIndex);

				if (pointAbsolute < nHoverIndex * 40 + 20) {
					//winMain.textTemp.Text = string.Format("{0}번째의 앞에 : {1} {2}", nHoverIndex, pointMouseMove.Y, pointAbsolute);
					nToIndex = nHoverIndex;
				} else if (pointAbsolute >= nHoverIndex * 40 + 20) {
					//winMain.textTemp.Text = string.Format("{0}번째의 뒤에 : {1} {2}", nHoverIndex, pointMouseMove.Y, pointAbsolute);
					nToIndex = nHoverIndex + 1;
				}

				winMain.rectMovePosition.Margin = new Thickness(0, nToIndex * 40 - winMain.scrollList.VerticalOffset, 0, 0);
			}
		}

		public static int nPrevMovingIndex = -1;
		public static void MouseUp() {
			isMouseDown = false;
			if (nMouseDownID < 0 || !isMoving) { return; }
			isMoving = false;
			winMain.gridMoveStatus.Visibility = Visibility.Collapsed;

			nPrevMovingIndex = nMouseDownID;
			if (nToIndex < 0) { return; }

			if (SongData.DictSong[nMouseDownID].nPosition < nToIndex) { nToIndex--; }
			if (SongData.DictSong[nMouseDownID].nPosition == nToIndex) { nMouseDownID = -1; return; }


			winMain.stackList.Children.Remove(SongData.DictSong[nMouseDownID].gBase);
			winMain.stackList.Children.Insert(nToIndex, SongData.DictSong[nMouseDownID].gBase);

			nMouseDownID = -1;

			Pref.isSorted = false;
			PlayClass.RefreshSongPosition();
			FileIO.SaveSongList();

			winMain.gridIndexer.Visibility = Visibility.Collapsed;
			winMain.buttonIndexerSort.Visibility = Visibility.Visible;
			if (Pref.isAutoSort) {
				winMain.ShowMessage("자동 정렬 옵션이 해제되었습니다.", 4);
				winMain.buttonAutoSortOff.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			}
		}

		static bool isCornerScrollingDelay = false;
		private static void DelayTimer(double time, string idTag) {
			DispatcherTimer timerDelay = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(time), IsEnabled = true, Tag = idTag };
			timerDelay.Tick += timerDelay_Tick;
		}

		private static void timerDelay_Tick(object sender, EventArgs e) {
			string id = (string)((DispatcherTimer)sender).Tag;

			switch (id) {
				case "isCornerScrollingDelay":
					isCornerScrollingDelay = false;
					((DispatcherTimer)sender).Stop();
					break;
			}
			CalculatePoint();
		}
	}
}
