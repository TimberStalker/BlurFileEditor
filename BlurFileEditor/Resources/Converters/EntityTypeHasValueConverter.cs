using BlurFormats.BlurData;
using BlurFormats.BlurData.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters
{
    public class EntityTypeHasValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IEntity entity) return Visibility.Collapsed;

            if (entity.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPrimitiveEntity<>)))
            {
                return Visibility.Visible;
            } else if (entity is EnumEntity or FlagsEnumEntity)
            {
                return Visibility.Visible;
            }

            return !string.IsNullOrEmpty(entity.Type.FormatString) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
