using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters
{
    [ValueConversion(typeof(object), typeof(bool), ParameterType = typeof(Type))]
    public class ValueImplementsTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.GetType().IsAssignableTo(parameter as Type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
