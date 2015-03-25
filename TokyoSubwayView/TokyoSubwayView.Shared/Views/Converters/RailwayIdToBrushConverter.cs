using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TokyoSubwayView.Models.Metro;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TokyoSubwayView.Views.Converters
{
	public class RailwayIdToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if ((MetroManager.Current.RailwayIds == null) ||
				(!MetroManager.Current.RailwayIds.Contains(value.ToString())))
				return DependencyProperty.UnsetValue;

			return MetroManager.Current.RailwayIdBrushMap[value.ToString()];
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}