using BlurFormats.BlurData;
using BlurFormats.BlurData.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters;
public class FieldToDataTypeDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var field = (DataField)value;

        StringBuilder result = new StringBuilder(field.DataType!.Name);

        if(field.FieldType is FieldType.Pointer or FieldType.PointerArray)
        {
            result.Append("*");
        }
        else if(field.FieldType is FieldType.ExternalPointer or FieldType.ExternalArray)
        {
            result.Append("^");
        }
        if((int)field.FieldType >= 256)
        {
            result.Append("[]");
        }
        return result.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
