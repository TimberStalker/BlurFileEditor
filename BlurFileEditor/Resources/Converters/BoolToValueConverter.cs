using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters
{
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T? FalseValue { get; set; }
        public T? TrueValue { get; set; }

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool b) return FalseValue;
            return b ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : BoolToValueConverter<Visibility> { }
}
