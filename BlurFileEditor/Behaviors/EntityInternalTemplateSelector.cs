using BlurFormats.BlurData.Entities;
using BlurFormats.BlurData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BlurFileEditor.Behaviors
{
    class EntityInternalTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            IEntity entity = (IEntity)item;

            DataTemplate? template = null;

            if (entity is FlagsEnumEntity)
            {
                template = (DataTemplate)((FrameworkElement)container).TryFindResource(typeof(FlagsEnumEntity));
            }
            else if(entity is EnumEntity)
            {
                template = (DataTemplate)((FrameworkElement)container).TryFindResource(typeof(EnumEntity));
            }
            else if (entity.GetType().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPrimitiveEntity<>)))
            {
                var inter = item.GetType().GetInterfaces().First(i => i.GetGenericTypeDefinition() == typeof(IPrimitiveEntity<>));
                var structType = inter.GetGenericArguments()[0];
                template = (DataTemplate)((FrameworkElement)container).TryFindResource(inter.GetGenericArguments()[0]);
            }
            else
            {
                template = (DataTemplate)((FrameworkElement)container).TryFindResource(typeof(ObjectEntity));
            }

            template ??= (DataTemplate)((FrameworkElement)container).FindResource("DefaultEntityTemplate");

            return template;
        }
    }
}
