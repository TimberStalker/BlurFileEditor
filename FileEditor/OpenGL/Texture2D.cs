using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata;
using System.Runtime.Versioning;
using Editor;
using Editor.OpenGL;
using Hexa.NET.ImGui;
using Hexa.NET.OpenGL;
using SkiaSharp;
using static OpenGL;

public unsafe sealed class Texture2D : GLObject
{
    public Texture2D() : base(GL3.GenTexture())
    {
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

        using (var tex = texture.Bind())
        {
            tex.WrapS = GLTextureWrapMode.ClampToEdge;
            tex.WrapT = GLTextureWrapMode.ClampToEdge;
            tex.MinFilter = GLTextureMinFilter.Linear;
            tex.MagFilter = GLTextureMagFilter.Linear;

            tex.SetBits(bitmap);
        }
        return texture;
    }
    protected override void Dispose(bool disposing)
    {
        GL3.DeleteTexture(Handle);
    }
    public static implicit operator ImTextureID(Texture2D texture) => (ImTextureID)texture.Handle;
    public static implicit operator ImTextureRef(Texture2D texture) => new(texId: texture);

    public Texture2DReference Bind(bool generateMipmap = false)
    {
        return new Texture2DReference(Handle, generateMipmap);
    }
}

public ref struct Texture2DReference
{
    uint lastHandle;
    public readonly int Width
    {
        get
        {
            GL3.GetTexLevelParameteriv(GLTextureTarget.Texture2D, 0, GLGetTextureParameter.Width, out var width);
            return width;
        }
    }
    public readonly int Height
    {
        get
        {
            GL3.GetTexLevelParameteriv(GLTextureTarget.Texture2D, 0, GLGetTextureParameter.Height, out var height);
            return height;
        }
    }
    public readonly GLTextureWrapMode WrapS
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.WrapS, (int)value);
        }
    }
    public readonly GLTextureWrapMode WrapT
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.WrapT, (int)value);
        }
    }
    public readonly GLTextureMinFilter MinFilter
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MinFilter, (int)value);
        }
    }
    public readonly GLTextureMagFilter MagFilter
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.Texture2D, GLTextureParameterName.MagFilter, (int)value);
        }
    }

    public readonly void SetBits(SKBitmap bitmap, bool generateMipmap = true)
    {
        using var destBimap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        bitmap.CopyTo(destBimap, SKColorType.Bgra8888);
        GL3.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgba32F, destBimap.Width, destBimap.Height, 0, GLPixelFormat.Bgra, GLPixelType.UnsignedByte, destBimap.GetPixelSpan());

        if (generateMipmap)
            GL3.GenerateMipmap(GLTextureTarget.Texture2D);
    }
    public Texture2DReference(uint handle, bool restoreAfterUsing)
    {
        if (restoreAfterUsing)
        {
            GL3.GetIntegeri_v(GLGetPName.TextureBinding2D, 0, out var lastHandle);
            this.lastHandle = (uint)lastHandle;
        }
        else
        {
            this.lastHandle = uint.MaxValue;
        }
        GL3.BindTexture(GLTextureTarget.Texture2D, handle);
    }
    public void Dispose()
    {
        if (lastHandle != uint.MaxValue)
            GL3.BindTexture(GLTextureTarget.Texture2D, lastHandle);
    }
}