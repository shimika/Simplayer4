using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Simplayer4 {
	/// <summary>
	/// ChangeNotification.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class ChangeNotification : Window {
		string before, after, sortinfo;

		public ChangeNotification(string b, string a, string s) {
			InitializeComponent();

			this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left + 25;
			this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top + 150;
			Loaded += delegate(object sender, RoutedEventArgs e) { new AltTab().HideAltTab(this); };
			buttonClose.Click += buttonClose_Click;

			before = b; after = a; sortinfo = s;
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e) { this.Close(); }
		public void AnimateWindow() {
			textBefore.Text = before;
			textAfter.Text = after;
			textScript.Text = sortinfo;

			this.Topmost = false;
			this.Topmost = true;
			Storyboard sb = new Storyboard() { Duration = TimeSpan.FromSeconds(5) };

			DoubleAnimation fin, fout;
			fin = new DoubleAnimation(1, TimeSpan.FromMilliseconds(250));
			fout = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250));

			fout.BeginTime = TimeSpan.FromMilliseconds(4000);

			Storyboard.SetTarget(fin, this); Storyboard.SetTarget(fout, this);

			Storyboard.SetTargetProperty(fin, new PropertyPath(Window.OpacityProperty));
			Storyboard.SetTargetProperty(fout, new PropertyPath(Window.OpacityProperty));

			sb.Children.Add(fin); sb.Children.Add(fout);
			sb.Completed += sb_Completed;

			sb.Begin(this);
		}

		private void sb_Completed(object sender, EventArgs e) {
			this.Close();
		}
	}
}
