using DirectXTexNet;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    Texture2D texture;
    public DirectXImageWindow(string path)
    {
        TexturePath = path;

        var sImage = TexHelper.Instance.LoadFromDDSFile(path, DDS_FLAGS.NONE);
        var metadata = sImage.GetMetadata();
        if(TexHelper.Instance.IsCompressed(metadata.Format))
        {
            var temp = sImage.Decompress(DXGI_FORMAT.UNKNOWN);
            sImage.Dispose();
            sImage = temp;
            metadata = sImage.GetMetadata();
        }
        using var stream = sImage.SaveToWICMemory(0, WIC_FLAGS.NONE, TexHelper.Instance.GetWICCodec(WICCodecs.PNG));
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);
        texture = Texture2D.CreateFromBytes(bytes);
        sImage.Dispose();
    }

    public bool Draw()
    {
        bool open = true;
        if (ImGui.Begin($"{Path.GetFileName(TexturePath)}##{TexturePath}", ref open, ImGuiWindowFlags.NoCollapse))
        {
            var size = ImGui.GetWindowSize();
            var minSize = MathF.Max(MathF.Min(size.X, size.Y) - 60, 80);

            int width = texture.Width;
            int height = texture.Height;

            if(width >= height)
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

            ImGui.Image(texture, new System.Numerics.Vector2(width, height));
            ImGui.End();
        }
        return open;
    }
}
