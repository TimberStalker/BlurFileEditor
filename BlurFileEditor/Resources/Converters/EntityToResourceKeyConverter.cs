using BlurFormats.BlurData.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters
{
    public class EntityToResourceKeyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var entity = (IEntity)value;
            if (entity.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPrimitiveEntity<>)))
            {
                var primitve = entity.GetType().GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IPrimitiveEntity<>));
                return primitve.GenericTypeArguments[0];
            }
            if(entity is ReferenceEntity r)
            {
                return r.Entity.Type;
            }
            return entity.Type;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
