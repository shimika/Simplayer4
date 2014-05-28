using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		public int nFromIndex, nToIndex, nMouseDownID, nMouseMovingID;
		public bool isMoving, isMouseDown;
		public Point pointMouseDown, pointMouseMove;

		public void MousePressDown(int nId, Point pPoint) {
			nPrevMovingIndex = -1;

			isMouseDown = true; isMoving = false;
			nMouseDownID = nId; nToIndex = -1;
			pointMouseDown = pPoint;
			nFromIndex = SongData.DictSong[nId].Position;
			textNowMoving.Text = SongData.DictSong[nId].Title;
		}

		public void MouseMove(Point point) {
			if (!isMouseDown || nMouseDownID < 0) { return; }
			pointMouseMove = point;
			
			if (Math.Max(Math.Abs(pointMouseDown.X - pointMouseMove.X), Math.Abs(pointMouseDown.Y - pointMouseMove.Y)) >= 10 && !isMoving) {
				nMouseMovingID = nMouseDownID;
				isMoving = true;

				gridMoveStatus.Visibility = Visibility.Visible;
			}

			if (!isMoving) { return; }
			CalculatePoint();
		}

		private void CalculatePoint() {
			if (!isMouseDown || nMouseDownID < 0 || !isMoving) { return; }

			if (pointMouseMove.X < 0 || pointMouseMove.X > gridListArea.ActualWidth) {
				nToIndex = -1;
				gridMoveStatus.Visibility = Visibility.Collapsed;
				return;
			} else {
				gridMoveStatus.Visibility = Visibility.Visible;
			}

			gridNowMoving.Margin = new Thickness(pointMouseMove.X - 100, pointMouseMove.Y - 20, 0, 0);

			if (pointMouseMove.Y < -10) {
				nToIndex = -1;
				rectMovePosition.Visibility = Visibility.Collapsed;

				if (isCornerScrollingDelay) { return; }
				isCornerScrollingDelay = true;
				DelayTimer(250, "isCornerScrollingDelay");

				scrollList.BeginAnimation(AniScrollViewer.CurrentVerticalOffsetProperty,
					new DoubleAnimation(scrollList.VerticalOffset, Math.Max(scrollList.VerticalOffset - 150, 0), TimeSpan.FromMilliseconds(200)) {
						EasingFunction = new ExponentialEase() { Exponent = 1, EasingMode = EasingMode.EaseInOut },
					});
			} else if (pointMouseMove.Y > gridListArea.ActualHeight + 10) {
				nToIndex = -1;
				rectMovePosition.Visibility = Visibility.Collapsed;

				if (isCornerScrollingDelay) { return; }
				isCornerScrollingDelay = true;
				DelayTimer(250, "isCornerScrollingDelay");

				scrollList.BeginAnimation(AniScrollViewer.CurrentVerticalOffsetProperty,
							new DoubleAnimation(scrollList.VerticalOffset, Math.Min(scrollList.VerticalOffset + 150, scrollList.ScrollableHeight), TimeSpan.FromMilliseconds(200)) {
								EasingFunction = new ExponentialEase() { Exponent = 1, EasingMode = EasingMode.EaseInOut },
							});
			} else {
				rectMovePosition.Visibility = Visibility.Visible;
				double pointAbsolute = scrollList.VerticalOffset + pointMouseMove.Y;
				int nHoverIndex = ((int)pointAbsolute) / 40;

				nHoverIndex = Math.Max(0, nHoverIndex);
				nHoverIndex = Math.Min(SongData.DictSong.Count - 1, nHoverIndex);

				if (pointAbsolute < nHoverIndex * 40 + 20) {
					//textTemp.Text = string.Format("{0}번째의 앞에 : {1} {2}", nHoverIndex, pointMouseMove.Y, pointAbsolute);
					nToIndex = nHoverIndex;
				} else if (pointAbsolute >= nHoverIndex * 40 + 20) {
					//textTemp.Text = string.Format("{0}번째의 뒤에 : {1} {2}", nHoverIndex, pointMouseMove.Y, pointAbsolute);
					nToIndex = nHoverIndex + 1;
				}

				rectMovePosition.Margin = new Thickness(0, nToIndex * 40 - scrollList.VerticalOffset, 0, 0);
			}
		}

		public int nPrevMovingIndex = -1;
		public void MouseReleaseUp() {
			isMouseDown = false;
			if (nMouseDownID < 0 || !isMoving) { return; }
			isMoving = false;
			gridMoveStatus.Visibility = Visibility.Collapsed;

			nPrevMovingIndex = nMouseDownID;
			if (nToIndex < 0) { return; }

			if (SongData.DictSong[nMouseDownID].Position < nToIndex) { nToIndex--; }

			if (SongData.DictSong[nMouseDownID].Position == nToIndex) { nMouseDownID = -1; return; }

			stackList.Children.Remove(SongData.DictSong[nMouseDownID].GridBase);
			stackList.Children.Insert(nToIndex, SongData.DictSong[nMouseDownID].GridBase);

			nMouseDownID = -1;

			Pref.isSorted = false;

			RefreshAbsolutePositionByStackPanel();
			SaveSongList();

			// Disable sorted + autosort
			gridIndexer.Visibility = Visibility.Collapsed;
			buttonIndexerSort.Visibility = Visibility.Visible;
			if (Pref.isAutoSort) {
				ShowMessage("자동 정렬 옵션이 해제되었습니다.", 4);
				buttonAutoSortOff.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
			}
		}

		bool isCornerScrollingDelay = false;
		private void DelayTimer(double time, string idTag) {
			DispatcherTimer timerDelay = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(time), IsEnabled = true, Tag = idTag };
			timerDelay.Tick += timerDelay_Tick;
		}

		private void timerDelay_Tick(object sender, EventArgs e) {
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
