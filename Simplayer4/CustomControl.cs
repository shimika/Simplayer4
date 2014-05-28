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
	public partial class MainWindow : Window {
		public SolidColorBrush sColor;

		public Grid GetListItemButton(SongData sData, bool isNew) {
			Grid grid = new Grid() {
				Height = 40, Tag = sData.ID,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};

			Button button = new Button() { Height = 40, Tag = sData.ID, Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Stretch };
			grid.MouseEnter += delegate(object sender, MouseEventArgs e) { ((Grid)sender).Background = new SolidColorBrush(Color.FromArgb(40, sColor.Color.R, sColor.Color.G, sColor.Color.B)); };
			grid.MouseLeave += delegate(object sender, MouseEventArgs e) { ((Grid)sender).Background = Brushes.Transparent; };

			ContextMenu context = new ContextMenu() { HasDropShadow = false };
			MenuItem mItem1 = new MenuItem() {
				Header = "폴더 열기 : " + sData.Title, Tag = sData.FilePath,
				HeaderStringFormat = "폴더 열기 : {0}",
			};
			Binding binding0 = new Binding("Title");
			binding0.Source = SongData.DictSong[sData.ID];
			mItem1.SetBinding(MenuItem.HeaderProperty, binding0);

			context.Items.Add(mItem1);
			MenuItem mItem2 = new MenuItem() { Header = "삭제", Tag = sData.ID };
			context.Items.Add(mItem2);
			grid.ContextMenu = context;

			mItem1.Click += (o, e) => {
				string filePath = (string)((MenuItem)o).Tag;
				if (!System.IO.File.Exists(filePath)) { return; }
				string argument = @"/select, " + filePath;

				gridContextBlock.Visibility = Visibility.Collapsed;
				Task.Factory.StartNew(() => Process.Start("explorer.exe", argument));
			};
			mItem2.Click += (o, e) => {
				gridContextBlock.Visibility = Visibility.Collapsed;
				FileDelete((int)((MenuItem)o).Tag);
			};

			TextBlock txt1 = new TextBlock() {
				Text = sData.Title, Margin = new Thickness(40, 0, 50, 0), FontSize = 13.33,
				HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center,
				TextTrimming = TextTrimming.CharacterEllipsis,
			};

			Binding binding1 = new Binding("Title");
			binding1.Source = SongData.DictSong[sData.ID];
			txt1.SetBinding(TextBlock.TextProperty, binding1);

			TextBlock txt2 = new TextBlock() {
				Text = sData.DurationString, Margin = new Thickness(5, 0, 25, 0), FontSize = 10, Foreground = Brushes.Gray,
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
			grid.PreviewMouseDown += gridBase_MouseDown;

			grid.Children.Add(txt1);
			grid.Children.Add(txt2);
			grid.Children.Add(rect);
			grid.Children.Add(button);
			grid.Children.Add(gridPlayImage);
			grid.Children.Add(polyNew);

			return grid;
		}
		private void gridBase_MouseDown(object sender, MouseButtonEventArgs e) {
			if (e.LeftButton == MouseButtonState.Pressed) {
				MousePressDown((int)((Grid)sender).Tag, e.GetPosition(gridListArea));
			}
		}
	}
}
