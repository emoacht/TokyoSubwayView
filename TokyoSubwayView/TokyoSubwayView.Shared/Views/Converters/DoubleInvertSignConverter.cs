using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace TokyoSubwayView.Views.Converters
{
    public class DoubleInvertSignConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is double))
                return DependencyProperty.UnsetValue;

            return (double)value * -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (!(value is double))
                return DependencyProperty.UnsetValue;

            return (double)value * -1;
        }
    }
}