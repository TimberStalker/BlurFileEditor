using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;
using System.IO;

namespace BlurFileEditor.Utils.Interop.Files;
public static class FileManager
{
    public static ImageSource GetImageSource(string filename)
    {
        return GetImageSource(filename, new Size(16, 16));
    }

    public static ImageSource GetImageSource(string filename, Size size)
    {
        using (var icon = ShellManager.GetIcon(Path.GetExtension(filename), ItemType.File, IconSize.Small, ItemState.Undefined))
        {
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(size.Width, size.Height));
        }
    }
}