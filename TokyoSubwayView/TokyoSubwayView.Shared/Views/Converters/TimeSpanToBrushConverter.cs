using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TokyoSubwayView.Views.Converters
{
	public class TimeSpanToBrushConverter : IValueConverter
	{
		private static Brush[] _brushes;

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			double buff;
			if (!(value is TimeSpan) || !(double.TryParse(parameter.ToString(), out buff)))
				return DependencyProperty.UnsetValue;

			if (_brushes == null)
			{
				_brushes = new[]
				{
					(Brush)Application.Current.Resources["App.TimePicker.NormalBrush"],
					(Brush)Application.Current.Resources["App.TimePicker.WarningBrush"],
				};
			}

			return ((TimeSpan)value < TimeSpan.FromMinutes(buff)) ? _brushes[0] : _brushes[1];
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}