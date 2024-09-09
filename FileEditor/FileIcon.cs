using System.Runtime.InteropServices;
using SkiaSharp;
using Gdk;
using System.Runtime.Versioning;
using System.Drawing;
using Gtk;

public sealed class FileIcon
{
    [SupportedOSPlatform("windows")]
    static SKBitmap GetIconBitmapWindows(string filePath)
    {
        var icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath)!;
        if (icon == null)
        {
            throw new Exception("Icon not found.");
        }
        using Bitmap bitmap = icon.ToBitmap();
        using MemoryStream ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        byte[] imageData = ms.ToArray();

        return SKBitmap.Decode(imageData);
    }
    [SupportedOSPlatform("linux")]
    static SKBitmap GetIconBitmapLinux(string filePath)
    {
        // Get the file icon using Gdk
        IconLookupFlags flags = IconLookupFlags.UseBuiltin;
        IconTheme theme = IconTheme.Default;
        string iconName = theme.LookupIcon(filePath, 48, flags)?.Filename!;
        
        if (iconName == null)
        {
            throw new Exception("Icon not found.");
        }

        // Load the icon as a Pixbuf
        Pixbuf pixbuf = theme.LoadIcon(iconName, 48, flags);

        if (pixbuf == null)
        {
            throw new Exception("Failed to load icon.");
        }

        // Convert Pixbuf to byte array
        byte[] imageData = pixbuf.SaveToBuffer("png");

        // Create an SKBitmap from the byte array
        return SKBitmap.Decode(imageData);
    }
    public static SKBitmap GetIcon(string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetIconBitmapWindows(filePath);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetIconBitmapLinux(filePath);
        throw new NotSupportedException();
    }
}
