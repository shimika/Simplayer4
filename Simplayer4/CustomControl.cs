using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Simplayer4 {
	public class CustomControl {
		public static SolidColorBrush sColor;
		public static MainWindow winMain;

		public static Grid GetListItemButton(SongData sData, bool isNew) {
			Grid grid = new Grid() {
				Height = 40, Tag = sData.nID,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};

			Button button = new Button() { Height = 40, Tag = sData.nID, Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
			grid.MouseEnter += delegate(object sender, MouseEventArgs e) { ((Grid)sender).Background = new SolidColorBrush(Color.FromArgb(40, sColor.Color.R, sColor.Color.G, sColor.Color.B)); };
			grid.MouseLeave += delegate(object sender, MouseEventArgs e) { ((Grid)sender).Background = Brushes.Transparent; };

			ContextMenu context = new ContextMenu() { HasDropShadow = false };
			MenuItem mItem1 = new MenuItem() {
				Header = "폴더 열기 : " + sData.strTitle, Tag = sData.strFilePath,
				HeaderStringFormat = "폴더 열기 : {0}",
			};
			Binding binding0 = new Binding("strTitle");
			binding0.Source = SongData.DictSong[sData.nID];
			mItem1.SetBinding(MenuItem.HeaderProperty, binding0);

			context.Items.Add(mItem1);
			MenuItem mItem2 = new MenuItem() { Header = "삭제", Tag = sData.nID };
			context.Items.Add(mItem2);
			grid.ContextMenu = context;

			mItem1.Click += (o, e) => {
				string filePath = (string)((MenuItem)o).Tag;
				if (!System.IO.File.Exists(filePath)) { return; }
				string argument = @"/select, " + filePath;

				winMain.gridContextBlock.Visibility = Visibility.Collapsed;
				Task.Factory.StartNew(() => Process.Start("explorer.exe", argument));
			};
			mItem2.Click += (o, e) => {
				winMain.gridContextBlock.Visibility = Visibility.Collapsed;
				winMain.DeleteProcess((int)((MenuItem)o).Tag);
			};

			TextBlock txt1 = new TextBlock() {
				Text = sData.strTitle, Margin = new Thickness(40, 0, 50, 0), FontSize = 13.33,
				HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center,
				TextTrimming = TextTrimming.CharacterEllipsis,
			};

			Binding binding1 = new Binding("strTitle");
			binding1.Source = SongData.DictSong[sData.nID];
			txt1.SetBinding(TextBlock.TextProperty, binding1);

			TextBlock txt2 = new TextBlock() {
				Text = sData.strDuration, Margin = new Thickness(5, 0, 25, 0), FontSize = 10, Foreground = Brushes.Gray,
				HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Center,
			};
			Rectangle rect = new Rectangle() {
				Height = 1, VerticalAlignment = VerticalAlignment.Bottom,
				Opacity = 0.3, Margin = new Thickness(15, 0, 15, 0),
			};
			rect.SetResourceReference(Rectangle.FillProperty, "sColor");

			Grid gridPlayImage = new Grid() {
				Width = 24, Height = 24,
				HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(10, 4, 0, 0),
				Visibility = Visibility.Visible,
			};

			Polygon polyNew = new Polygon() { HorizontalAlignment = HorizontalAlignment.Left, Visibility = Visibility.Collapsed, Opacity = 0.7 };
			polyNew.SetResourceReference(Polygon.FillProperty, "sColor");
			polyNew.Points.Add(new Point(0, 10));
			polyNew.Points.Add(new Point(0, 40));
			polyNew.Points.Add(new Point(20, 40));

			if (isNew) { polyNew.Visibility = Visibility.Visible; }
			grid.PreviewMouseDown += grid_MouseDown;

			grid.Children.Add(txt1);
			grid.Children.Add(txt2);
			grid.Children.Add(rect);
			grid.Children.Add(button);
			grid.Children.Add(gridPlayImage);
			grid.Children.Add(polyNew);

			return grid;
		}

		static void grid_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.LeftButton == MouseButtonState.Pressed) {
				ReArrange.MouseDown((int)((Grid)sender).Tag, e.GetPosition(winMain.gridListArea));
			}
		}

		public static BitmapImage rtSource(string uriSource) {
			uriSource = "pack://application:,,,/Simplayer4;component/Resources/" + uriSource;
			BitmapImage source = new BitmapImage(new Uri(uriSource));
			return source;
		}
	}
}
