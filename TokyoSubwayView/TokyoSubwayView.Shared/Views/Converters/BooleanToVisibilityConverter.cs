using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TokyoSubwayView.Views.Converters
{
	public class BooleanToVisibilityConverter : IValueConverter
	{
		public bool IsReversed { get; set; } // Default is false.

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is bool))
				return DependencyProperty.UnsetValue;

			var buff = (bool)value;

			return (buff != IsReversed) ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if (!(value is Visibility))
				return DependencyProperty.UnsetValue;

			var buff = ((Visibility)value == Visibility.Visible);

			return (buff != IsReversed);
		}
	}
}