using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		// Global variables

		public NotifyIcon TrayNotify = new System.Windows.Forms.NotifyIcon();
		private ToolStripMenuItem LyrMenuItem = new System.Windows.Forms.ToolStripMenuItem("가사창 표시");
		private ToolStripMenuItem CloseMenuItem = new System.Windows.Forms.ToolStripMenuItem("닫기");

		private void SetNotifyIcon() {
			IntPtr iconHandle = Simplayer4.Properties.Resources.Music2.Handle;
			TrayNotify.Icon = System.Drawing.Icon.FromHandle(iconHandle);
			TrayNotify.Visible = true; TrayNotify.Text = "Simplayer4";

			ContextMenuStrip contextStrip = new ContextMenuStrip();
			contextStrip.Items.Add(LyrMenuItem);
			contextStrip.Items.Add(CloseMenuItem);
			TrayNotify.ContextMenuStrip = contextStrip;

			TrayNotify.MouseDoubleClick += TrayNotify_MouseDoubleClick;
			LyrMenuItem.Click += LyrWindow_Click;
			CloseMenuItem.Click += MenuShutdown_Click;
		}

		private void TrayNotify_MouseDoubleClick(object sender, MouseEventArgs e) {
			if (e.Button == System.Windows.Forms.MouseButtons.Left) {
				ActivateWindow();
			}
		}
		private void LyrWindow_Click(object sender, EventArgs e) {
			Pref.isLyricsVisible = !Pref.isLyricsVisible;

			buttonLyricsOn.Visibility = Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
			buttonLyricsOff.Visibility = !Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;

			LyricsWindow.ToggleLyrics(Pref.isLyricsVisible);
			LyrMenuItem.Checked = Pref.isLyricsVisible;
			SavePreference();
		}
		private void MenuShutdown_Click(object sender, EventArgs e) {
			this.Close();
		}
	}
}
