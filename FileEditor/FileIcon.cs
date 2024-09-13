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
    [SupportedOSPlatform("windows")]
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
        GLib.IFile file = GLib.FileFactory.NewForPath(filePath);
        //var fileInfo = file.QueryInfo("standard::icon", 0, new GLib.Cancellable());
        using GLib.FileIcon icon = new GLib.FileIcon(file);

        string type = "";
        using var iconStram = icon.Load(64, type, new GLib.Cancellable());
        using Pixbuf pixbuf = new Pixbuf(iconStram, new GLib.Cancellable());
        
        if (pixbuf == null)
        {
            throw new Exception("Failed to load icon.");
        }
        byte[] imageData = pixbuf.SaveToBuffer("png");

        return SKBitmap.Decode(icon.File.Path);
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
