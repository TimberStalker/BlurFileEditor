using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BlurFileFormats.SerializationFramework;
using DirectXTexNet;
using Editor.OpenGL;
using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.Vulkan;
using Hexa.NET.OpenGL;
using Pango;
using SkiaSharp;
using static OpenGL;

namespace Editor.Windows;
public class ImageWindow : GuiWindow, IDisposable
{
    public string TexturePath { get; }
    Texture2D texture;
    public ImageWindow(string path)
    {
        TexturePath = path;

        texture = Texture2D.CreateFromFile(path);
    }

    public bool Draw()
    {
        bool open = true;
        if (ImGui.Begin($"{Path.GetFileName(TexturePath)}##{TexturePath}", ref open, ImGuiWindowFlags.NoCollapse))
        {
            var size = ImGui.GetWindowSize();
            var minSize = MathF.Min(size.X, size.Y);
            ImGui.Image(texture, new Vector2(minSize, minSize));
        }
        ImGui.End();
        return open;
    }

    public void Dispose()
    {
        texture.Dispose();
    }
}
public class DirectXImageWindow : GuiWindow, IDisposable
{
    public string TexturePath { get; }
    TextureDisplay Display { get; }
    public DirectXImageWindow(string path)
    {
        TexturePath = path;

        var sImage = TexHelper.Instance.LoadFromDDSFile(path, DDS_FLAGS.NONE);
        var metadata = sImage.GetMetadata();
        if (TexHelper.Instance.IsCompressed(metadata.Format))
        {
            var temp = sImage.Decompress(DXGI_FORMAT.UNKNOWN);
            sImage.Dispose();
            sImage = temp;
            metadata = sImage.GetMetadata();
        }
        if(metadata.IsCubemap())
        {
            Display = new CubemapImage(sImage, metadata);
        }
        else
        {
            Display = new SingleImage(sImage, metadata);
        }
        sImage.Dispose();
    }

    public bool Draw()
    {
        bool open = true;
        if (ImGui.Begin($"{Path.GetFileName(TexturePath)}##{TexturePath}", ref open, ImGuiWindowFlags.NoCollapse))
        {
            var size = ImGui.GetWindowSize();
            if(ImGui.BeginChild(2, size - new Vector2(20, 40), ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                Display.Draw();
            }
                ImGui.EndChild();
        }
            ImGui.End();
        return open;
    }


    public void Dispose()
    {
        if (Display is IDisposable d) d.Dispose();
    }
    interface TextureDisplay
    {
        public void Draw();
    }
    class SingleImage : TextureDisplay, IDisposable
    {
        Texture2D texture;
        float scale = 0;

        public SingleImage(ScratchImage sImage, TexMetadata metadata)
        {
            using var stream = sImage.SaveToWICMemory(0, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            texture = Texture2D.CreateFromBytes(bytes);
            sImage.Dispose();
        }
        Vector2 offset;
        Vector2 padding = new Vector2(20, 20);
        Vector2 halfPadding = new Vector2(10, 10);
        public void Draw()
        {
            if(ImGui.BeginChild("container", Vector2.Zero, ImGuiChildFlags.None, ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                var size = ImGui.GetWindowSize();
                var minSize = MathF.Max(MathF.Min(size.X, size.Y) - 60, 80);

                Vector2 offset = new Vector2(size.X / 2, size.Y / 2);

                var cursorScreenPos = ImGui.GetCursorScreenPos();
                if (ImGui.IsWindowHovered())
                {
                    scale += ImGui.GetIO().MouseWheel / 4;

                    if(ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                    {
                        var delta = ImGui.GetIO().MouseDelta;
                        ImGui.SetScrollX(ImGui.GetScrollX() - delta.X);
                        ImGui.SetScrollY(ImGui.GetScrollY() - delta.Y);
                    }
                }

                int width;
                int height;
                using(var tex = texture.Bind())
                {
                    width = tex.Width;
                    height = tex.Height;
                }

                if (width >= height)
                {
                    float scale = minSize / width;
                    width = (int)minSize;
                    height = (int)(height * scale);
                }
                else
                {
                    float scale = minSize / height;
                    height = (int)minSize;
                    width = (int)(width * scale);
                }

                ImGui.Image(texture, new Vector2(width, height) * MathF.Exp(scale));
            }
                ImGui.EndChild();

        }

        public void Dispose()
        {
            texture.Dispose();
        }
    }
    class CubemapImage : TextureDisplay, IDisposable
    {
        CubemapTexture texture;
        Texture2D renderTexture;
        FrameBuffer frameBuffer;
        Shader shader;
        float pitch;
        float yaw;
        const float standardFov = 65 * MathF.PI / 180;
        float fov = standardFov;
        uint vao;
        uint vbo;
        public CubemapImage(ScratchImage sImage, TexMetadata metadata)
        {
            var bitmaps = new SKBitmap[metadata.ArraySize];
            
            for(int i = 0; i < metadata.ArraySize; i++)
            {
                using var stream = sImage.SaveToWICMemory(i, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                bitmaps[i] = SKBitmap.Decode(bytes);
            }
            texture = CubemapTexture.CreateFromBitmaps(bitmaps);


            vao = GL3.GenVertexArray();
            GL3.BindVertexArray(vao);

            vbo = GL3.GenBuffer();
            GL3.BindBuffer(GLBufferTargetARB.ArrayBuffer, vbo);
            GL3.BufferData(GLBufferTargetARB.ArrayBuffer, sizeof(float) * skyboxVertices.Length, skyboxVertices.AsSpan(), GLBufferUsageARB.StaticDraw); 
            GL3.VertexAttribPointer(0, 3, GLVertexAttribPointerType.Float, false, 0, 0);
            GL3.EnableVertexAttribArray(0);
            
            GL3.BindVertexArray(0);
            
            shader = Shader.Create(Path.Combine(Environment.CurrentDirectory, "Shaders", "skybox"));

            renderTexture = new Texture2D();
            using (var tex = renderTexture.Bind())
            {
                tex.MinFilter = GLTextureMinFilter.Linear;
                tex.MagFilter = GLTextureMagFilter.Linear;
                GL3.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgb, 1, 1, 0, GLPixelFormat.Rgb, GLPixelType.UnsignedByte, 0);
            }

            frameBuffer = new FrameBuffer();
            using(var fb = frameBuffer.Bind())
            {
                fb.AttachTexture(GLFramebufferAttachment.ColorAttachment0, renderTexture);
            }
        }
        Vector2 lastSize;
        Vector2 padding = new Vector2(20, 20);
        Vector2 halfPadding = new Vector2(10, 10);
        public void Draw()
        {
            if(ImGui.BeginChild("container", Vector2.Zero, ImGuiChildFlags.None))
            {
                var size = ImGui.GetWindowSize() - padding;
                if(lastSize != size)
                {
                    GL3.BindTexture(GLTextureTarget.Texture2D, renderTexture);
                    GL3.TexImage2D(GLTextureTarget.Texture2D, 0, GLInternalFormat.Rgb, (int)size.X, (int)size.Y, 0, GLPixelFormat.Rgb, GLPixelType.UnsignedByte, 0);
                    lastSize = size;
                }
                var minSize = MathF.Max(MathF.Min(size.X, size.Y) - 60, 80);

                if(ImGui.IsWindowHovered())
                {
                    if (ImGui.IsMouseDown(ImGuiMouseButton.Middle))
                    {
                        var mouseDrag = ImGui.GetIO().MouseDelta / 6 * standardFov;
                        yaw -= mouseDrag.X;
                        pitch -= mouseDrag.Y;
                        pitch = (float)Math.Clamp(pitch, -90, 90);
                        //rotation *= Quaternion.CreateFromYawPitchRoll(mouseDrag.X, mouseDrag.Y, 0);
                        //ImGui.GetIO().WantSetMousePos = true;
                        //ImGui.GetIO().MousePos = ImGui.GetIO().MousePosPrev;
                    }
                    fov += (ImGui.GetIO().MouseWheel / 50);
                    fov = Math.Clamp(fov, 0.1f, 3.13f);
                }

                var drawList = ImGui.GetWindowDrawList();

                int width;
                int height;

                using(var tex = texture.Bind())
                {
                    width = tex.Width;
                    height = tex.Height;
                }

                if (width >= height)
                {
                    float scale = minSize / width;
                    width = (int)minSize;
                    height = (int)(height * scale);
                }
                else
                {
                    float scale = minSize / height;
                    height = (int)minSize;
                    width = (int)(width * scale);
                }
                var pitchQuat = Quaternion.CreateFromYawPitchRoll(0, pitch * MathF.PI / 180, 0);
                var rotation = pitchQuat * Quaternion.CreateFromYawPitchRoll(yaw * MathF.PI / 180, 0, 0);
            
                var viewMatrix = Matrix4x4.CreateFromQuaternion(rotation);
                var perspective = Matrix4x4.CreatePerspectiveFieldOfView(fov, size.X/size.Y, 0.1f, 100f);
            
            
                GL3.BindFramebuffer(GLFramebufferTarget.Framebuffer, frameBuffer);
                //GL3.ClipControl(GL.GL_LOWER_LEFT, GL.GL_NEGATIVE_ONE_TO_ONE);
                GL3.ActiveTexture(GLTextureUnit.Texture0);
                GL3.BindTexture(GLTextureTarget.CubeMap, texture);
                shader.Use();
                shader.SetMatrix("projection", perspective);
                shader.SetMatrix("view", viewMatrix);
                shader.SetInt("skybox", 0);
            
                GL3.BindVertexArray(vao);
            
                GL3.Disable(GLEnableCap.DepthTest);
                GL3.Viewport(0, 0, (int)size.X, (int)size.Y);
                GL3.ClearColor(0, 0, 0, 1);
                GL3.Clear(GLClearBufferMask.ColorBufferBit | GLClearBufferMask.DepthBufferBit);
                GL3.DrawArrays(GLPrimitiveType.Triangles, 0, 36);
            
                GL3.UseProgram(0);
                GL3.BindVertexArray(0);
                GL3.BindFramebuffer(GLFramebufferTarget.Framebuffer, 0);

                ImGui.SetCursorPos(halfPadding);
                ImGui.Image(renderTexture, new Vector2(size.X, size.Y));
                ImGui.EndChild();
            }
        }

        public void Dispose()
        {
            frameBuffer.Dispose();
            texture.Dispose();
            renderTexture.Dispose();
        }

        float[] skyboxVertices = {
    -10.0f,  10.0f, -10.0f,
    -10.0f, -10.0f, -10.0f,
     10.0f, -10.0f, -10.0f,
     10.0f, -10.0f, -10.0f,
     10.0f,  10.0f, -10.0f,
    -10.0f,  10.0f, -10.0f,

    -10.0f, -10.0f,  10.0f,
    -10.0f, -10.0f, -10.0f,
    -10.0f,  10.0f, -10.0f,
    -10.0f,  10.0f, -10.0f,
    -10.0f,  10.0f,  10.0f,
    -10.0f, -10.0f,  10.0f,

     10.0f, -10.0f, -10.0f,
     10.0f, -10.0f,  10.0f,
     10.0f,  10.0f,  10.0f,
     10.0f,  10.0f,  10.0f,
     10.0f,  10.0f, -10.0f,
     10.0f, -10.0f, -10.0f,

    -10.0f, -10.0f,  10.0f,
    -10.0f,  10.0f,  10.0f,
     10.0f,  10.0f,  10.0f,
     10.0f,  10.0f,  10.0f,
     10.0f, -10.0f,  10.0f,
    -10.0f, -10.0f,  10.0f,

    -10.0f,  10.0f, -10.0f,
     10.0f,  10.0f, -10.0f,
     10.0f,  10.0f,  10.0f,
     10.0f,  10.0f,  10.0f,
    -10.0f,  10.0f,  10.0f,
    -10.0f,  10.0f, -10.0f,

    -10.0f, -10.0f, -10.0f,
    -10.0f, -10.0f,  10.0f,
     10.0f, -10.0f, -10.0f,
     10.0f, -10.0f, -10.0f,
    -10.0f, -10.0f,  10.0f,
     10.0f, -10.0f,  10.0f
};
    }
}
