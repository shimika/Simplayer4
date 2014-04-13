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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;

namespace Simplayer4 {
	public class CondensedTextBlock : FrameworkElement {

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			"Text", typeof(String), typeof(CondensedTextBlock), new FrameworkPropertyMetadata(
				 "", FrameworkPropertyMetadataOptions.AffectsRender));

		public String Text {
			get { return (String)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); InvalidateVisual(); }
		}

		public FontFamily FontFamily {
			get { return (FontFamily)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); InvalidateVisual(); }
		}

		public static readonly DependencyProperty FontFamilyProperty =
			DependencyProperty.Register("FontFamily", typeof(FontFamily),
			typeof(CondensedTextBlock),
			new FrameworkPropertyMetadata(new FontFamily("Segoe UI"), FrameworkPropertyMetadataOptions.AffectsRender));


		public Double FontStretch {
			get { return (double)GetValue(FontStretchProperty); }
			set { SetValue(FontStretchProperty, value); InvalidateVisual(); }
		}

		public static readonly DependencyProperty FontStretchProperty =
			DependencyProperty.Register("FontStretch", typeof(Double),
			typeof(CondensedTextBlock),
			new FrameworkPropertyMetadata((double)100, FrameworkPropertyMetadataOptions.AffectsRender));


		public Brush Foreground {
			get { return (Brush)GetValue(ForegroundProperty); }
			set { SetValue(ForegroundProperty, value); InvalidateVisual(); }
		}

		public static readonly DependencyProperty ForegroundProperty =
			DependencyProperty.Register("Foreground", typeof(Brush),
			typeof(CondensedTextBlock),
			new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));


		public double FontSize {
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); Height = value; }
		}

		public static readonly DependencyProperty FontSizeProperty =
			DependencyProperty.Register("FontSize", typeof(double),
			typeof(CondensedTextBlock),
			new FrameworkPropertyMetadata((double)11, FrameworkPropertyMetadataOptions.AffectsRender));


		protected override void OnRender(DrawingContext dc) {
			DrawingContext DrawingContext = dc;

			double XOffset = 0;
			if (Text == null) return;
			foreach (char Char in Text) {
				FormattedText FormattedText = new FormattedText(Char.ToString(),
					CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
					new Typeface(FontFamily, FontStyles.Normal, FontWeights.Bold,
					FontStretches.Normal), FontSize, Foreground);

				DrawingContext.DrawText(FormattedText, new Point(XOffset, 0));

				Geometry textGeometry = FormattedText.BuildGeometry(new Point(XOffset, 0));
				DrawingContext.DrawGeometry(Brushes.White, new Pen(new SolidColorBrush(Colors.Black), 0.2), textGeometry);
				XOffset += FormattedText.WidthIncludingTrailingWhitespace * FontStretch / 100;
			}

		}

		protected override Size MeasureOverride(Size availableSize) {
			availableSize.Height = FontSize * 1.2;
			return availableSize;
		}
	}
}
