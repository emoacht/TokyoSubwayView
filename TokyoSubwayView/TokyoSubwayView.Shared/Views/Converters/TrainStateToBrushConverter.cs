using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TokyoSubwayView.Models.Metro;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TokyoSubwayView.Views.Converters
{
	public class TrainStateToBrushConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (MetroManager.Current.TrainStateBrushMap == null)
				return DependencyProperty.UnsetValue;

			TrainState buff;
			if (!Enum.TryParse(value.ToString(), out buff))
				return DependencyProperty.UnsetValue;

			return MetroManager.Current.TrainStateBrushMap[buff];
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			throw new NotImplementedException();
		}
	}
}