using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters
{
    class MultiSelectComboBoxItemSelectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int value = (int)values[0];
            var combo = (ComboBox)values[1];
            if (values[2] is not ComboBoxItem item) return false;

            int index = combo.ItemContainerGenerator.IndexFromContainer(item);
            return (value & (1 << index)) > 0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
