using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Drawing;

namespace BlurFileEditor.Utils.Interop.Files;
public static class FolderManager
{
    public static ImageSource GetImageSource(string directory, ItemState folderType)
    {
        return GetImageSource(directory, new Size(16, 16), folderType);
    }

    public static ImageSource GetImageSource(string directory, Size size, ItemState folderType)
    {
        using (var icon = ShellManager.GetIcon(directory, ItemType.Folder, IconSize.Large, folderType))
        {
            return Imaging.CreateBitmapSourceFromHIcon(icon.Handle,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromWidthAndHeight(size.Width, size.Height));
        }
    }
}
