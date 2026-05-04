using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.OpenGL;
using SkiaSharp;
using static OpenGL;

namespace Editor.OpenGL;
public class FrameBuffer : GLObject
{
    public FrameBuffer() : base(GL3.GenFramebuffer())
    {
    }

    protected override void Dispose(bool disposing)
    {
        GL3.BindTexture(GLTextureTarget.Texture2D, 0);
        GL3.DeleteTexture(Handle);
    }
    public FrameBufferReference Bind(bool restoreAfterUsing = true)
    {
        return new FrameBufferReference(Handle, restoreAfterUsing);
    }
}

public ref struct FrameBufferReference
{
    uint lastReadHandle;
    uint lastDrawHandle;

    public void AttachTexture(GLFramebufferAttachment attachment, Texture2D texture, int mipmapLevel = 0)
    {
        GL3.FramebufferTexture2D(GLFramebufferTarget.Framebuffer, attachment, GLTextureTarget.Texture2D, texture, mipmapLevel);
        if (GL3.CheckFramebufferStatus(GLFramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new Exception("Cubemap framebuffer is not complete");
        }
    }

    public FrameBufferReference(uint handle, bool restoreAfterUsing)
    {
        if (restoreAfterUsing)
        {
            GL3.GetIntegeri_v(GLGetPName.ReadFramebufferBinding, 0, out var lastReadHandle);
            this.lastReadHandle = (uint)lastReadHandle;
            GL3.GetIntegeri_v(GLGetPName.DrawFramebufferBinding, 0, out var lastDrawHandle);
            this.lastDrawHandle = (uint)lastDrawHandle;
        }
        else
        {
            this.lastReadHandle = uint.MaxValue;
            this.lastDrawHandle = uint.MaxValue;
        }
        GL3.BindFramebuffer(GLFramebufferTarget.Framebuffer, handle);
    }
    public void Dispose()
    {
        if (lastReadHandle != uint.MaxValue)
            GL3.BindFramebuffer(GLFramebufferTarget.ReadFramebuffer, lastReadHandle);
        if (lastDrawHandle != uint.MaxValue)
            GL3.BindFramebuffer(GLFramebufferTarget.DrawFramebuffer, lastDrawHandle);
    }
}