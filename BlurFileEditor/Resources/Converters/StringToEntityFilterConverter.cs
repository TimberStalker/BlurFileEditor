using BlurFileEditor.Utils.FIlter;
using BlurFileEditor.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace BlurFileEditor.Resources.Converters
{
    class StringToEntityFilterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string s) throw new Exception();
            if (string.IsNullOrWhiteSpace(s))
            {
                return null!;
            }
            var filter = FilterParser.CreateFilter(s);
            return filter;
        }
    }
}
