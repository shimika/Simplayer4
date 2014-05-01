using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Simplayer4 {
	/// <summary>
	/// PreviewWindow1.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class PreviewWindow : Window {
		public const int WS_EX_TRANSPARENT = 0x00000020;
		public const int GWL_EXSTYLE = (-20);
		[DllImport("user32.dll")]
		public static extern int GetWindowLong(IntPtr hwnd, int index);
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			IntPtr hwnd = new WindowInteropHelper(this).Handle;
			int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
			SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
		}

		public PreviewWindow() {
			InitializeComponent();
			this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left + 100;
			this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Bottom - 200;
			this.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - 150;
			Loaded += delegate(object sender, RoutedEventArgs e) { new AltTab().HideAltTab(this); };
			dtm.Tick += dtm_Tick;
		}

		bool isOpened = false;
		DispatcherTimer dtm = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(3000), IsEnabled = false };
		double AnimateTime = 500, EasingK = 6;

		public void AnimateWindow(string title, string sub, bool isAnimated = true) {
			if (!isAnimated && gridContainer.Children.Count > 0) {
				((gridContainer.Children[gridContainer.Children.Count - 1] as StackPanel).Children[0] as CondensedTextBlock).Text = title;
				((gridContainer.Children[gridContainer.Children.Count - 1] as StackPanel).Children[1] as CondensedTextBlock).Text = sub;
				return;
			}

			dtm.Stop();

			this.Topmost = false;
			this.Topmost = true;

			Storyboard sb = new Storyboard();

			if (gridContainer.Children.Count > 0) {
				DoubleAnimation daBefore = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
				Storyboard.SetTarget(daBefore, gridContainer.Children[gridContainer.Children.Count - 1]);
				Storyboard.SetTargetProperty(daBefore, new PropertyPath(StackPanel.OpacityProperty));

				ThicknessAnimation taBefore = new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(200)) {
					EasingFunction = new ExponentialEase() {
						EasingMode = EasingMode.EaseIn, Exponent = 2,
					}
				};
				Storyboard.SetTarget(taBefore, gridContainer.Children[gridContainer.Children.Count - 1]);
				Storyboard.SetTargetProperty(taBefore, new PropertyPath(StackPanel.MarginProperty));

				sb.Children.Add(daBefore);
				sb.Children.Add(taBefore);
			}

			// Create new info

			StackPanel stackBase = new StackPanel() { Margin = new Thickness(50, 0, 0, 0), Opacity = 1 };
			CondensedTextBlock txt1 = new CondensedTextBlock() { FontSize = 30, Foreground = Brushes.White, Text = title, Height = 40, FontStretch = 250, Margin = new Thickness(60, 0, 0, 0), Opacity = 0 };
			CondensedTextBlock txt2 = new CondensedTextBlock() { FontSize = 20, Foreground = Brushes.White, Text = sub, Height = 30, FontStretch = 250, Margin = new Thickness(80, 0, 0, 0), Opacity = 0 };

			stackBase.Children.Add(txt1);
			stackBase.Children.Add(txt2);

			// Fade in, out animation

			DoubleAnimation fin, fout;
			fin = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250));
			fout = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250)) { BeginTime = TimeSpan.FromMilliseconds(3500) };

			Storyboard.SetTarget(fin, this);
			Storyboard.SetTargetProperty(fin, new PropertyPath(Window.OpacityProperty));

			Storyboard.SetTarget(fout, this);
			Storyboard.SetTargetProperty(fout, new PropertyPath(Window.OpacityProperty));

			// New info animation

			DoubleAnimation daStretch1, daStretch2, daFade1, daFade2;
			ThicknessAnimation taMargin1, taMargin2;

			daStretch1 = new DoubleAnimation(250, 100, TimeSpan.FromMilliseconds(AnimateTime)) {
				EasingFunction = new ExponentialEase() {
					EasingMode = EasingMode.EaseOut,
					Exponent = EasingK,
				},
				BeginTime = TimeSpan.FromMilliseconds(250)
			};
			daStretch2 = new DoubleAnimation(250, 100, TimeSpan.FromMilliseconds(AnimateTime)) {
				EasingFunction = new ExponentialEase() {
					EasingMode = EasingMode.EaseOut,
					Exponent = EasingK,
				},
				BeginTime = TimeSpan.FromMilliseconds(250)
			};
			Storyboard.SetTarget(daStretch1, txt1);
			Storyboard.SetTargetProperty(daStretch1, new PropertyPath(CondensedTextBlock.FontStretchProperty));
			Storyboard.SetTarget(daStretch2, txt2);
			Storyboard.SetTargetProperty(daStretch2, new PropertyPath(CondensedTextBlock.FontStretchProperty));

			taMargin1 = new ThicknessAnimation(new Thickness(0), TimeSpan.FromMilliseconds(AnimateTime)) {
				EasingFunction = new ExponentialEase() {
					EasingMode = EasingMode.EaseOut,
					Exponent = EasingK,
				},
				BeginTime = TimeSpan.FromMilliseconds(250)
			};
			taMargin2 = new ThicknessAnimation(new Thickness(20, 0, 0, 0), TimeSpan.FromMilliseconds(AnimateTime)) {
				EasingFunction = new ExponentialEase() {
					EasingMode = EasingMode.EaseOut,
					Exponent = EasingK,
				},
				BeginTime = TimeSpan.FromMilliseconds(450)
			};
			Storyboard.SetTarget(taMargin1, txt1);
			Storyboard.SetTargetProperty(taMargin1, new PropertyPath(CondensedTextBlock.MarginProperty)); 
			Storyboard.SetTarget(taMargin2, txt2);
			Storyboard.SetTargetProperty(taMargin2, new PropertyPath(CondensedTextBlock.MarginProperty));

			daFade1 = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)) { BeginTime = TimeSpan.FromMilliseconds(250) };
			daFade2 = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200)) { BeginTime = TimeSpan.FromMilliseconds(450) };

			Storyboard.SetTarget(daFade1, txt1);
			Storyboard.SetTargetProperty(daFade1, new PropertyPath(TextBlock.OpacityProperty));
			Storyboard.SetTarget(daFade2, txt2);
			Storyboard.SetTargetProperty(daFade2, new PropertyPath(TextBlock.OpacityProperty)); 

			if (!isOpened) {
				sb.Children.Add(fin);
				//sb.Children.Add(slong);
			}
			gridContainer.Children.Add(stackBase);
			isOpened = true;
			dtm.Start();

			sb.Children.Add(fout);
			//sb.Children.Add(sshort);

			sb.Children.Add(daStretch1);
			sb.Children.Add(daStretch2);
			sb.Children.Add(taMargin1);
			sb.Children.Add(taMargin2);
			sb.Children.Add(daFade1);
			sb.Children.Add(daFade2);

			sb.Begin(this);
		}

		private void dtm_Tick(object sender, EventArgs e) {
			(sender as DispatcherTimer).Stop();
			isOpened = false;
		}
	}
}
