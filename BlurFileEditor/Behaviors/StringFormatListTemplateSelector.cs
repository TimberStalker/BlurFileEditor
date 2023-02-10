using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BlurFileEditor.Behaviors
{
    public class StringFormatListTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {

            var labelTemplate = (DataTemplate)((FrameworkElement)container).FindResource("LabelTemplate");
            var contentTemplate = (DataTemplate)((FrameworkElement)container).FindResource("TypeContentTemplate");

            if(item is string)
            {
                return labelTemplate;
            }
            else
            {
                return contentTemplate;
            }
        }
    }
}
