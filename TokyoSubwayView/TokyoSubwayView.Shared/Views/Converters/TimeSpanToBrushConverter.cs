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
		private static Brush[] brushes;

		public object Convert(object value, Type targetType, object parameter, string language)
		{
			double buff;
			if (!(value is TimeSpan) || !(double.TryParse(parameter.ToString(), out buff)))
				return DependencyProperty.UnsetValue;

			if (brushes == null)
			{
				brushes = new[]
				{
					(Brush)Application.Current.Resources["App.TimePicker.NormalBrush"],
					(Brush)Application.Current.Resources["App.TimePicker.WarningBrush"],
				};
			}

			return ((TimeSpan)value < TimeSpan.FromMinutes(buff)) ? brushes[0] : brushes[1];
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}