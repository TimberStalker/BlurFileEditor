using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BlurFileEditor.Utils.Dragging;
public class DraggedObjectAdorner : Adorner
{
    UIElement visual;
    RenderTargetBitmap imagePreview;
    public DraggedObjectAdorner(UIElement adornedElement) : base(adornedElement)
    {
        var template = DragAdorner.GetAdornerTemplate(adornedElement);

        var visual = new ContentPresenter()
        {
            Content = adornedElement,
            ContentTemplate = template
        };
        visual.ApplyTemplate();
        imagePreview = new RenderTargetBitmap(90 + 5, 40, 96, 96, PixelFormats.Default);
        this.visual = visual;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        imagePreview.Render(visual);
        var brush = new ImageBrush(imagePreview)
        {
            Opacity = 0.5
        };
        drawingContext.DrawRectangle(brush, new Pen(), new Rect(RenderSize));
    }
}
