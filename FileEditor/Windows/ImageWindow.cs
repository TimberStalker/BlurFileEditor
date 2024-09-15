using BlurFileFormats.SerializationFramework;
using DirectXTexNet;
using Editor.OpenGL;
using Editor.Rendering;
using ImGuiNET;
using Pango;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace Editor.Windows;
public class ImageWindow : IWindow
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
            ImGui.Image(texture, new System.Numerics.Vector2(minSize, minSize));
            ImGui.End();
        }
        return open;
    }
}
public class DirectXImageWindow : IWindow
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
            ImGui.BeginChildFrame(2, size - new Vector2(20, 40), ImGuiWindowFlags.HorizontalScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
            Display.Draw();
            ImGui.End();
        }
        return open;
    }
    interface TextureDisplay
    {
        public void Draw();
    }
    class SingleImage : TextureDisplay
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

        public void Draw()
        {
            scale += ImGui.GetIO().MouseWheel / 4;

            if(ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                var delta = ImGui.GetIO().MouseDelta;
                ImGui.SetScrollX(ImGui.GetScrollX() - delta.X);
                ImGui.SetScrollY(ImGui.GetScrollY() - delta.Y);
            }

            var drawList = ImGui.GetWindowDrawList();

            var size = ImGui.GetWindowSize();
            var minSize = MathF.Max(MathF.Min(size.X, size.Y) - 60, 80);

            int width = texture.Width;
            int height = texture.Height;

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
    }
    class CubemapImage : TextureDisplay
    {
        CubemapTexture texture;
        Texture2D renderTexture;
        FrameBuffer frameBuffer;
        Shader shader;
        float pitch;
        float yaw;
        float fov = MathF.PI * 4 / 5;
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


            GL.glGenVertexArrays(1, out vao);
            GL.glBindVertexArray(vao);

            GL.glGenBuffers(GL.GL_ARRAY_BUFFER, out vbo);
            GL.glBindBuffer(GL.GL_ARRAY_BUFFER, vbo);
            GL.glBufferData(GL.GL_ARRAY_BUFFER, (ulong)(sizeof(float) * skyboxVertices.Length), skyboxVertices, GL.GL_STATIC_DRAW); 
            GL.glVertexAttribPointer(0, 3, GL.GL_FLOAT, false, 6 * sizeof(float), 0);
            GL.glEnableVertexAttribArray(0);

            GL.glBindVertexArray(0);

            shader = Shader.Create(Path.Combine(Environment.CurrentDirectory, "Shaders", "skybox"));

            frameBuffer = new FrameBuffer();
            GL.glBindFramebuffer(GL.GL_FRAMEBUFFER, frameBuffer);

            renderTexture = new Texture2D();
            GL.glBindTexture(GL.GL_TEXTURE_2D, renderTexture);
            GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, GL.GL_RGB, 400, 400, 0, GL.GL_RGB, GL.GL_UNSIGNED_BYTE, 0);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
            GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);
            
            frameBuffer.AttactTexture(renderTexture);

            if (GL.glCheckFramebufferStatus(GL.GL_FRAMEBUFFER) != GL.GL_FRAMEBUFFER_COMPLETE)
            {
                throw new Exception("Cubemap framebuffer is not complete");
            }

            GL.glBindFramebuffer(GL.GL_FRAMEBUFFER, 0);

            GLDrawings.Execute += () =>
            {
                try
                {
                    var pitchQuat = Quaternion.CreateFromYawPitchRoll(0, pitch * MathF.PI / 180, 0);
                    var rotation = pitchQuat * Quaternion.CreateFromYawPitchRoll(yaw * MathF.PI / 180, 0, 0);

                    var viewMatrix = Matrix4x4.CreateFromQuaternion(rotation);
                    var perspective = Matrix4x4.CreatePerspectiveFieldOfView(fov, 1, 0.1f, 100f);

                    shader.Use();
                    shader.SetMatrix("projection", perspective);
                    shader.SetMatrix("view", viewMatrix);

                    GL.glBindFramebuffer(GL.GL_FRAMEBUFFER, frameBuffer);



                    GL.glBindVertexArray(vao);
                    GL.glBindTexture(GL.GL_TEXTURE_CUBE_MAP, texture);
                    GL.glDrawArrays(GL.GL_TRIANGLES, 0, 36);

                    GL.glBindVertexArray(0);
                    GL.glBindFramebuffer(GL.GL_FRAMEBUFFER, 0);
                } catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            };
        }
        public void Draw()
        {
            var size = ImGui.GetWindowSize();
            var minSize = MathF.Max(MathF.Min(size.X, size.Y) - 60, 80);
            Vector2 offset = new Vector2(size.X / 2, size.Y / 2);

            if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                var mouseDrag = ImGui.GetIO().MouseDelta / 10;
                yaw += mouseDrag.X;
                pitch += mouseDrag.Y;
                //rotation *= Quaternion.CreateFromYawPitchRoll(mouseDrag.X, mouseDrag.Y, 0);
            }
            fov += (ImGui.GetIO().MouseWheel / 50);
            fov = Math.Clamp(fov, 0.1f, 3.13f);

            //var drawList = ImGui.GetWindowDrawList();

            int width = texture.Width;
            int height = texture.Height;

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

            ImGui.Image(texture, new Vector2(width, height));
        }
        float[] skyboxVertices = {
    -1.0f,  1.0f, -1.0f,
    -1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,

    -1.0f, -1.0f,  1.0f,
    -1.0f, -1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f,  1.0f,
    -1.0f, -1.0f,  1.0f,

     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,

    -1.0f, -1.0f,  1.0f,
    -1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f, -1.0f,  1.0f,
    -1.0f, -1.0f,  1.0f,

    -1.0f,  1.0f, -1.0f,
     1.0f,  1.0f, -1.0f,
     1.0f,  1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
    -1.0f,  1.0f,  1.0f,
    -1.0f,  1.0f, -1.0f,

    -1.0f, -1.0f, -1.0f,
    -1.0f, -1.0f,  1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
    -1.0f, -1.0f,  1.0f,
     1.0f, -1.0f,  1.0f
};
    }
}
