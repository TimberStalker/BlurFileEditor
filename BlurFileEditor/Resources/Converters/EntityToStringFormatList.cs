using BlurFormats.BlurData.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BlurFileEditor.Resources.Converters
{
    public class EntityToStringFormatList : IValueConverter
    {
        static Regex templateSelectorRegex = new Regex(@"(?<!\\)\{(?'name'.+?)(?<!\\)\}");
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ObjectEntity entity) return null;
            if (string.IsNullOrEmpty(entity.Type.FormatString)) return null;
            var split = templateSelectorRegex.Replace(entity.Type.FormatString, "\x0").Split('\x0');
            var matches = templateSelectorRegex.Matches(entity.Type.FormatString);
            List<object> result = new List<object>();
            for(int i = 0; i < split.Length; i++)
            {
                if (!string.IsNullOrEmpty(split[i]))
                {
                    result.Add(split[i]);
                }
                if(i < matches.Count)
                {
                    result.Add(entity[matches[i].Groups["name"].Value]!.Value);
                }
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
