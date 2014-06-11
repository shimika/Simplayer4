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
	/// </summary>ba
	public partial class MainWindow : Window {

		#region Global variables

		public SolidColorBrush MainBrush;
		Color MainColor = Colors.SlateBlue;

		public PreviewWindow PrevWindow;
		public LyricsWindow LyricsWindow;

		string Version = "ver 4.1.1";
		public Grid GridNowPlay = null;
		ImageBrush[] ImgNowPlayArray = new ImageBrush[3];

		#endregion

		public MainWindow() {
			InitializeComponent();

			this.Left = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Right - 400;
			this.Top = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2 - 300;
		}

		private void Window_Loaded(object senderW, RoutedEventArgs eW) {
			sColor = MainBrush = FindResource("sColor") as SolidColorBrush;

			SetEventHandlers();
			SetNotifyIcon();
			IndexerPreset();

			StartPlayingEvent();
			SetPreferenceEvent();
			SetWindowEvent();

			ReadPreference();
			SetPreferenceToControl();
			InitPlayer();
		}

		List<SongData> ListSong = new List<SongData>();
		private void InitPlayer() {
			// Version check
			textArtist.Text = Version;

			// Make another windows
			PrevWindow = new PreviewWindow();
			PrevWindow.Show();

			LyricsWindow = new LyricsWindow(Pref.isLyricsVisible);
			LyricsWindow.Show();

			// Read songs + Make controls
			List<SongData> listInput = ReadSongList();
			AddToLibrary(listInput, false);

			// Activate window + finish initialize
			ActivateWindow();
			stackList.BeginAnimation(StackPanel.OpacityProperty,
				new DoubleAnimation(1, TimeSpan.FromMilliseconds(300)));

			ShuffleList();

			// Init playing icon image
			for (int i = 0; i < 3; i++) {
				ImgNowPlayArray[i] = new ImageBrush(rtSource(string.Format("iconPlaying{0}.png", i)));
			}

			GridNowPlay = new Grid() {
				Width = 24, Height = 24,
				HorizontalAlignment = HorizontalAlignment.Left,
			};
			GridNowPlay.SetResourceReference(Grid.BackgroundProperty, "sColor");
			GridNowPlay.OpacityMask = ImgNowPlayArray[0];

			GC.Collect();
		}
	}
}
