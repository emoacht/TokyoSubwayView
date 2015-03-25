using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TokyoSubwayView.Views.Converters
{
	public class EnumToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			if (!(value is Enum))
				return DependencyProperty.UnsetValue;

			return value.ToString();
		}

		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			if ((value == null) || !targetType.GetTypeInfo().IsEnum)
				return DependencyProperty.UnsetValue;

			var name = Enum.GetNames(targetType)
				.FirstOrDefault(x => x.ToString().Equals(value.ToString(), StringComparison.OrdinalIgnoreCase));

			return name ?? DependencyProperty.UnsetValue;
		}
	}
}