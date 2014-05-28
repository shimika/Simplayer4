using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Simplayer4 {
	public partial class MainWindow : Window {
		// Window Position
		[DllImport("user32.dll")]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		const UInt32 SWP_NOSIZE = 0x0001;
		const UInt32 SWP_NOMOVE = 0x0002;

		static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
		static void SendWpfWindowBack(Window window) {
			var hWnd = new WindowInteropHelper(window).Handle;
			SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE);
		}

		// Global hotkey

		HwndSource HWndSource;
		private int PlayPauseKey, Stopkey, Prevkey, Nextkey, Lyrkey, SynPrevkey, SynNextkey, LaunchKey;

		private void SetWindowEvent() {
			WindowInteropHelper wih = new WindowInteropHelper(this);
			HWndSource = HwndSource.FromHwnd(wih.Handle);
			HWndSource.AddHook(MainWindowProc);

			PlayPauseKey = WinAPI.GlobalAddAtom("ButtonPP");
			Stopkey = WinAPI.GlobalAddAtom("ButtonStop");
			Prevkey = WinAPI.GlobalAddAtom("ButtonPrev");
			Nextkey = WinAPI.GlobalAddAtom("ButtonNext");
			Lyrkey = WinAPI.GlobalAddAtom("LyricsKey");
			SynPrevkey = WinAPI.GlobalAddAtom("SyncPrevKey");
			SynNextkey = WinAPI.GlobalAddAtom("SyncNextKey");
			LaunchKey = WinAPI.GlobalAddAtom("ListShowHide");
			WinAPI.RegisterHotKey(wih.Handle, PlayPauseKey, 3, WinAPI.VK_DOWN);
			WinAPI.RegisterHotKey(wih.Handle, Stopkey, 3, WinAPI.VK_UP);
			WinAPI.RegisterHotKey(wih.Handle, Prevkey, 3, WinAPI.VK_LEFT);
			WinAPI.RegisterHotKey(wih.Handle, Nextkey, 3, WinAPI.VK_RIGHT);
			WinAPI.RegisterHotKey(wih.Handle, Lyrkey, 3, WinAPI.VK_KEY_D);
			WinAPI.RegisterHotKey(wih.Handle, SynPrevkey, 3, WinAPI.VK_OEM_COMMA);
			WinAPI.RegisterHotKey(wih.Handle, SynNextkey, 3, WinAPI.VK_OEM_PERIOD);
			WinAPI.RegisterHotKey(wih.Handle, LaunchKey, 8, WinAPI.VK_KEY_W);
		}

		private IntPtr MainWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			if (msg == WinAPI.WM_HOTKEY && Pref.isHotkeyOn) {
				if (wParam.ToString() == PlayPauseKey.ToString()) {
					TogglePlayingStatus();
				} else if (wParam.ToString() == Stopkey.ToString()) {
					StopPlayer();
				} else if (wParam.ToString() == Prevkey.ToString()) {
					if (SongData.NowPlaying >= 0) {
						MusicPrepare(SongData.NowPlaying, -1 * Pref.RandomSeed, true);
					}
				} else if (wParam.ToString() == Nextkey.ToString()) {
					if (SongData.NowPlaying >= 0) {
						MusicPrepare(SongData.NowPlaying, Pref.RandomSeed, true);
					}
				} else if (wParam.ToString() == Lyrkey.ToString()) {
					Pref.isLyricsVisible = !Pref.isLyricsVisible;
					buttonLyricsOn.Visibility = Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;
					buttonLyricsOff.Visibility = !Pref.isLyricsVisible ? Visibility.Visible : Visibility.Collapsed;

					LyricsWindow.ToggleLyrics(Pref.isLyricsVisible);
					LyrMenuItem.Checked = Pref.isLyricsVisible;
					SavePreference();
				} else if (wParam.ToString() == SynPrevkey.ToString()) {
					if (Pref.isPlaying != 0) {
						LyricsWindow.ChangeOffset(-200);
					}
				} else if (wParam.ToString() == SynNextkey.ToString()) {
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
	}
}
