using BlurFormats.BlurData;
using BlurFormats.BlurData.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters;
public class FieldHasDataTypeDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var field = (DataField)value;

        if (field.FieldType is FieldType.Pointer or FieldType.PointerArray or FieldType.ExternalPointer or FieldType.ExternalArray)
        {
            return Visibility.Visible;
        }
        if ((int)field.FieldType >= 256)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
