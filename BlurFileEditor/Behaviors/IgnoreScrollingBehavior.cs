using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace BlurFileEditor.Behaviors;
public class IgnoreScrollingBehavior
{
    public static readonly DependencyProperty IgnoreScrollingProperty = DependencyProperty.RegisterAttached("IgnoreScrolling", typeof(bool), typeof(IgnoreScrollingBehavior), new UIPropertyMetadata(false, OnIgnoreScrollingChanged));

    public static bool GetIgnoreScrolling(UIElement element)
    {
        return (bool)element.GetValue(IgnoreScrollingProperty);
    }
    public static void SetIgnoreScrolling(UIElement element, bool value)
    {
        element.SetValue(IgnoreScrollingProperty, value);
    }
    private static void OnIgnoreScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement i) return;

        if(e.NewValue is bool b)
        {
            if (b)
            {
                i.PreviewMouseWheel += OnPreviewMouseWheel;
            }
            else
            {
                i.PreviewMouseWheel -= OnPreviewMouseWheel;
            }
        }
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        MouseWheelEventArgs eventArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent,
            Source = sender
        };
        UIElement? parent = ((Control)sender).Parent as UIElement;
        parent?.RaiseEvent(eventArgs);
    }
}
