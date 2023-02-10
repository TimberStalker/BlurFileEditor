using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters
{
    public class ItemsFromFlagEnumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            List<object> list = new List<object>();

            if (values[0] is not IEnumerable items) return list;
            if (values[1] is not int value) return list;

            var enumerator = items.GetEnumerator();
            while (enumerator.MoveNext() && value > 0) 
            {
                var item = enumerator.Current;
                if((value & 1) > 0)
                {
                    list.Add(item);
                }
                value >>= 1;
            }
            return list;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
