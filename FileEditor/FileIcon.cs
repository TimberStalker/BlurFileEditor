using System.Runtime.InteropServices;
using SkiaSharp;
using Gdk;
using System.Runtime.Versioning;
using System.Drawing;
using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Shell;

public sealed class FileIcon
{
    [SupportedOSPlatform("windows5.1.2600")]
    unsafe static SKBitmap GetIconBitmapWindows(string filePath)
    {
        SHFILEINFOW fileInfo = new();
        PInvoke.SHGetFileInfo(filePath, FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_READONLY, &fileInfo, (uint)Marshal.SizeOf(fileInfo), SHGFI_FLAGS.SHGFI_ICON | SHGFI_FLAGS.SHGFI_SMALLICON);

        var icon = Icon.FromHandle(fileInfo.hIcon);

        if (icon == null)
        {
            throw new Exception("Icon not found.");
        }

        using Bitmap bitmap = icon.ToBitmap();
        using MemoryStream ms = new MemoryStream();
        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        byte[] imageData = ms.ToArray();

        PInvoke.DestroyIcon(fileInfo.hIcon);
        return SKBitmap.Decode(imageData);
    }
    [SupportedOSPlatform("linux")]
    static SKBitmap GetIconBitmapLinux(string filePath)
    {
        // Get the file icon using Gdk
        GLib.FileIcon icon = new GLib.FileIcon(GLib.FileFactory.NewForPath(filePath));

        // Load the icon as a Pixbuf
        Pixbuf pixbuf = new Pixbuf(icon.File.Path);

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
