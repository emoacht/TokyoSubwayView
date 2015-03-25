using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TokyoSubwayView.Views.Converters
{
	public class ColorToSolidColorBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is Color))
				return DependencyProperty.UnsetValue;

			return new SolidColorBrush((Color)value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (!(value is SolidColorBrush))
				return DependencyProperty.UnsetValue;

			return ((SolidColorBrush)value).Color;
		}
	}
}