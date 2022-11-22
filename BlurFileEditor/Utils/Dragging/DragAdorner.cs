using FileResearcher.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace BlurFileEditor.Utils.Dragging;
public static class DragAdorner
{
    public static readonly DependencyProperty AdornerTemplateProperty = DependencyProperty.RegisterAttached("AdornerTemplate", typeof(DataTemplate), typeof(DragAdorner), new UIPropertyMetadata(null, OnAdornerTemplateChanged));
    private static readonly DependencyPropertyKey DragAdornerProperty = DependencyProperty.RegisterAttachedReadOnly("DragAdorner", typeof(DraggedObjectAdorner), typeof(DragAdorner), new UIPropertyMetadata(null));

    public static void StartDrag(UIElement element)
    {
        SetDragAdorner(element, new DraggedObjectAdorner(element));
    }
    public static void EndDrag(UIElement element)
    {
        SetDragAdorner(element, null);
    }

    public static DataTemplate? GetAdornerTemplate(UIElement element)
    {
        return (DataTemplate?)element.GetValue(AdornerTemplateProperty);
    }
    public static void SetAdornerTemplate(UIElement element, DataTemplate? value)
    {
        element.SetValue(AdornerTemplateProperty, value);
    }

    private static DraggedObjectAdorner? GetDragAdorner(UIElement element)
    {
        return (DraggedObjectAdorner?)element.GetValue(DragAdornerProperty.DependencyProperty);
    }
    private static void SetDragAdorner(UIElement element, DraggedObjectAdorner? value)
    {
        element.SetValue(DragAdornerProperty, value);
    }

    private static void OnAdornerTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement u) return;

        if (e.NewValue is DataTemplate template)
        {
            u.GiveFeedback += OnGiveFeedback;
        } else
        {
            u.GiveFeedback -= OnGiveFeedback;
        }
    }

    private static void OnGiveFeedback(object source, GiveFeedbackEventArgs e)
    {
        if (source is not UIElement u) return;

        var draggedAdorner = GetDragAdorner(u);

        if (draggedAdorner is null) return;

        draggedAdorner.Arrange(new Rect(CursorHelper.GetCurrentCursorPositionRelativeTo(u), draggedAdorner.DesiredSize));

    }
}
