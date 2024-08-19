using BlurFileEditor.Utils.FIlter;
using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace BlurFileEditor.Resources.Converters;
public class MatchesFilterConverter : IMultiValueConverter
{

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2) return false;
        if (values[0] is not IEntityFilter Filter) return false;

        IEntity entity;
        if (values[1] is IEntity e)
        {
            entity = e;
        } else if (values[1] is StructureFieldValue sfv)
        {
            if (sfv.Entity is ReferenceEntity r)
            {
                entity = r.Reference;
            }
            else
            {
                entity = sfv.Entity;
            }
        } else if (values[1] is ArrayEntityItem ai)
        {
            entity = ((ReferenceEntity)ai.Reference).Reference;
        }
        else
        {
            return false;
        }
        return Filter.Matches(entity);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
