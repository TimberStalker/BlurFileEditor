using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Editor;
using GLib;
using Hexa.NET.ImGui;
using Hexa.NET.OpenGL;
using SkiaSharp;
using static OpenGL;

namespace Editor.OpenGL;
public class CubemapTexture : GLObject
{
    public CubemapTexture() : base(GL3.GenTexture())
    {
    }


    public static CubemapTexture CreateFromBitmaps(SKBitmap[] bitmaps)
    {
        Debug.Assert(bitmaps != null);
        Debug.Assert(bitmaps.Length != 6);

        var texture = new CubemapTexture();

        using (var tex = texture.Bind())
        {
            tex.WrapS = GLTextureWrapMode.ClampToEdge;
            tex.WrapT = GLTextureWrapMode.ClampToEdge;
            tex.WrapR = GLTextureWrapMode.ClampToEdge;

            tex.MinFilter = GLTextureMinFilter.Linear;
            tex.MagFilter = GLTextureMagFilter.Linear;

            for (uint i = 0; i < bitmaps.Length; i++)
            {
                SKBitmap? item = bitmaps[i];
                tex.SetBits(i, item);
            }
        }


        return texture;
    }

    bool disposed;
    private void Dispose(bool disposing)
    {
        if (disposed) return;
        disposed = true;
        if (disposing)
        {

        }
        GL3.DeleteTexture(Handle);
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    public static implicit operator ImTextureID(CubemapTexture texture) => (ImTextureID)texture.Handle;
    public unsafe static implicit operator ImTextureRef(CubemapTexture texture) => new(texId: texture);
    public CubemapTextureReference Bind(bool restoreAfterUsing = true)
    {
        return new CubemapTextureReference(Handle, restoreAfterUsing);
    }
}

public ref struct CubemapTextureReference
{
    uint lastHandle;
    public readonly int Width
    {
        get
        {
            GL3.GetTexLevelParameteriv(GLTextureTarget.CubeMap, 0, GLGetTextureParameter.Width, out var width);
            return width;
        }
    }
    public readonly int Height
    {
        get
        {
            GL3.GetTexLevelParameteriv(GLTextureTarget.CubeMap, 0, GLGetTextureParameter.Height, out var height);
            return height;
        }
    }
    public readonly GLTextureWrapMode WrapS
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.CubeMap, GLTextureParameterName.WrapS, (int)value);
        }
    }
    public readonly GLTextureWrapMode WrapT
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.CubeMap, GLTextureParameterName.WrapT, (int)value);
        }
    }
    public readonly GLTextureWrapMode WrapR
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.CubeMap, GLTextureParameterName.WrapT, (int)value);
        }
    }
    public readonly GLTextureMinFilter MinFilter
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.CubeMap, GLTextureParameterName.MinFilter, (int)value);
        }
    }
    public readonly GLTextureMagFilter MagFilter
    {
        set
        {
            GL3.TexParameteri(GLTextureTarget.CubeMap, GLTextureParameterName.MagFilter, (int)value);
        }
    }

    public readonly void SetBits(uint face, SKBitmap bitmap, bool generateMipmap = true)
    {
        using var destBimap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
        bitmap.CopyTo(destBimap, SKColorType.Bgra8888);
        GL3.TexImage2D(GLTextureTarget.CubeMapPositiveXExt + face, 0, GLInternalFormat.Rgba32F, destBimap.Width, destBimap.Height, 0, GLPixelFormat.Bgra, GLPixelType.UnsignedByte, destBimap.GetPixelSpan());

        if(generateMipmap)
            GL3.GenerateMipmap(GLTextureTarget.CubeMapPositiveXExt + face);
    }
    public CubemapTextureReference(uint handle, bool restoreAfterUsing)
    {
        if(restoreAfterUsing)
        {
            GL3.GetIntegeri_v(GLGetPName.TextureBindingCubeMap, 0, out var lastHandle);
            this.lastHandle = (uint)lastHandle;
        }
        else
        {
            this.lastHandle = uint.MaxValue;
        }
        GL3.BindTexture(GLTextureTarget.CubeMap, handle);
    }
    public void Dispose()
    {
        if(lastHandle != uint.MaxValue)
        GL3.BindTexture(GLTextureTarget.CubeMap, lastHandle);
    }
}