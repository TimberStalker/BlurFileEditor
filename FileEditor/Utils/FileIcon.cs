using System.Runtime.InteropServices;
using SkiaSharp;
using Gdk;
using System.Runtime.Versioning;
using System.Drawing;

using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Shell;
using GLib;
using Editor.Rendering;

public sealed class FileIcon
{
    static Dictionary<int, Texture2D> _icons = [];
    [SupportedOSPlatform("windows")]
    unsafe static Texture2D GetIconBitmapWindows(string filePath)
    {
        SHFILEINFOW fileInfo = new();
        PInvoke.SHGetFileInfo(filePath, FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_READONLY, &fileInfo, (uint)Marshal.SizeOf(fileInfo), SHGFI_FLAGS.SHGFI_ICON | SHGFI_FLAGS.SHGFI_SMALLICON);
        if(_icons.TryGetValue(fileInfo.iIcon, out var texture))
        {
            return texture;
        }
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
        return Texture2D.CreateFromBitmap(SKBitmap.Decode(imageData));
    }
    [SupportedOSPlatform("linux")]
    static Texture2D GetIconBitmapLinux(string filePath)
    {
        Cancellable cancellable = new Cancellable();

        IFile file = FileFactory.NewForPath(filePath);
        var fileInfo = file.QueryInfo("standard::icon", 0, cancellable);
        if(fileInfo.Icon is not ILoadableIcon icon)
        {
            throw new Exception("Failed to find icon.");
        }
        //using GLib.FileIcon icon = new GLib.FileIcon(file);

        string type = "";
        using var iconStream = icon.Load(64, type, cancellable);
        using Pixbuf pixbuf = new Pixbuf(iconStream, cancellable);
        
        if (pixbuf == null)
        {
            throw new Exception("Failed to load icon.");
        }
        byte[] imageData = pixbuf.SaveToBuffer("png");

        return Texture2D.CreateFromBitmap(SKBitmap.Decode(imageData));
    }

    public static Texture2D GetIcon(string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return GetIconBitmapWindows(filePath);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return GetIconBitmapLinux(filePath);
        throw new NotSupportedException();
    }
}
