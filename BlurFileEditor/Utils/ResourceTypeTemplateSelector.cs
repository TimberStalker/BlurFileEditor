using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BlurFileEditor.Utils
{
    public class ResourceTypeTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? DefaultTemplate { get; set; }
        public ResourceDictionary? TemplateDictionary { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(TemplateDictionary is not null)
            {
                var itemKey = new DataTemplateKey(item.GetType());
                if (TemplateDictionary.Contains(itemKey))
                {
                    if (TemplateDictionary[itemKey] is DataTemplate d) return d;
                }
            }
            return DefaultTemplate;
        }
    }
}
