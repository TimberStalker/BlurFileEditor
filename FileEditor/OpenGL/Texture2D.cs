using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using Editor;
using Editor.Rendering;
using SkiaSharp;

public unsafe sealed class Texture2D : IDisposable
{
    uint handle;
    bool setPixels;

    public int Width
    {
        get
        {
            GL.glGetTextureLevelParameteriv(handle, 0, GL.GL_TEXTURE_WIDTH, out var width);
            return width;
        }
    }
    public int Height
    {
        get
        {
            GL.glGetTextureLevelParameteriv(handle, 0, GL.GL_TEXTURE_HEIGHT, out var height);
            return height;
        }
    }

    public Texture2D()
    {
        GL.glGenTextures(1, out handle);
    }
    public void SetParameter(uint parameter, int value)
    {
        GL.glTextureParameteri(handle, parameter, value);
    }
    public void SetParameter(uint parameter, float value)
    {
        GL.glTextureParameterf(handle, parameter, value);
    }
    public void SetBits(SKBitmap bitmap)
    {
        GL.glBindTexture(GL.GL_TEXTURE_2D, handle);
        unsafe
        {
            using var destBimap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            bitmap.CopyTo(destBimap, SKColorType.Bgra8888);
            if(!setPixels)
            {
                GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA32F, destBimap.Width, destBimap.Height, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, destBimap.GetPixels());
                setPixels = true;
            }
            else
            {
                GL.glTexSubImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA32F, destBimap.Width, destBimap.Height, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, destBimap.GetPixels());
            }
            GL.glGenerateMipmap(GL.GL_TEXTURE_2D);
        }
    }
    [SupportedOSPlatform("windows")]
    public void SetBits(Bitmap bitmap)
    {
        GL.glBindTexture(GL.GL_TEXTURE_2D, handle);
        unsafe
        {
            var bData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            if (!setPixels)
            {
                GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA32F, bData.Width, bData.Height, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, bData.Scan0);
                setPixels = true;
            }
            else
            {
                GL.glTexSubImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGBA32F, bData.Width, bData.Height, 0, GL.GL_BGRA, GL.GL_UNSIGNED_BYTE, bData.Scan0);
            }
            GL.glGenerateMipmap(GL.GL_TEXTURE_2D);
            bitmap.UnlockBits(bData);
        }
    }

    public static Texture2D CreateFromFile(string file)
    {
        using SKBitmap bitmap = SKBitmap.Decode(file);
        return CreateFromBitmap(bitmap);
    }

    public static Texture2D CreateFromBytes(byte[] bytes)
    {
        using SKBitmap bitmap = SKBitmap.Decode(bytes);
        return CreateFromBitmap(bitmap);
    }

    public static Texture2D CreateFromBitmap(SKBitmap bitmap)
    {
        var texture = new Texture2D();

        texture.SetParameter(GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP_TO_EDGE);
        texture.SetParameter(GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP_TO_EDGE);
        texture.SetParameter(GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
        texture.SetParameter(GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);

        texture.SetBits(bitmap);
        return texture;
    }

    public static implicit operator uint(Texture2D texture)
    {
        return texture.handle;
    }
    public static implicit operator nint(Texture2D texture)
    {
        return (nint)texture.handle;
    }

    bool disposed;
    private void Dispose(bool disposing)
    {
        if (disposed) return;
        disposed = true;
        if(disposing)
        {

        }
        unsafe
        {
            GL.glBindTexture(GL.GL_TEXTURE_2D, 0);
            fixed(uint* ptr = &handle)
            {
                GL.glDeleteTextures(1, (uint)&ptr);
            }
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    ~Texture2D()
    {
        Program.ExecuteOnMainThread(Dispose, false);
    }
}