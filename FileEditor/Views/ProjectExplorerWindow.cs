using Editor;
using Editor.Drawers;
using Editor.Views;
using Editor.Windows;
using ImGuiNET;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

public class ProjectExplorerWindow : IView, IDisposable
{
    string filter = "";
    FileSystemObject? parentDirectory;
    public string Name => "Project Explorer";
    public string Id => "Project Explorer";
    public string? Shortcut => null;
    public event EventHandler<string>? OnOpenFile;
    bool open;
    bool focus;

    public void SetDirectory(string? directory)
    {
        if(directory is null)
        {
            parentDirectory = null;
        } else
        {
            parentDirectory = FileSystemObject.Create(directory);
        }
    }

    public void Draw()
    {
        if(focus)
        {
            ImGui.SetNextWindowFocus();
            focus = false;
        }
        if (ImGui.Begin("Project Explorer", ref open, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.InputText("##filterInput", ref filter, 100);
            if(parentDirectory is not null)
            {
                DrawFileItem(parentDirectory);
            }

            ImGui.End();
        }
    }
    public void Focus()
    {
        focus = true;
    }
    void DrawFileItem(FileSystemObject fileObject)
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.AllowOverlap | ImGuiTreeNodeFlags.SpanFullWidth;
        if (!fileObject.HasContents) flags |= ImGuiTreeNodeFlags.Leaf;

        bool showContents = ImGui.TreeNodeEx($"##{fileObject.Name}", flags);
        if(ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            OnOpenFile?.Invoke(this, fileObject.FilePath);
        }
        ImGui.SameLine();
        ImGui.Image(fileObject.Icon, new System.Numerics.Vector2(12, 12));
        ImGui.SameLine();
        ImGui.Text(fileObject.Name);
        if (showContents)
        {
            fileObject.PopulateContents();
            foreach (var content in fileObject.Contents)
            {
                DrawFileItem(content);
            }
            ImGui.TreePop();
        }
    }

    public void Dispose()
    {
        OnOpenFile = null;
    }
}
public class FileSystemObject
{
    public string Name { get; }
    public string FilePath { get; }
    public Texture2D Icon { get; }
    public bool HasContents => Contents is null || Contents.Length > 0;
    public FileSystemObject[]? Contents { get; private set; }
    FileSystemObject(string path, Texture2D icon)
    {
        Name = Path.GetFileName(path);
        FilePath = path;
        Icon = icon;
        var attributes = File.GetAttributes(path);
        if (!attributes.HasFlag(FileAttributes.Directory))
        {
            Contents = [];
        }
    }
    [MemberNotNull(nameof(Contents))]
    public void PopulateContents()
    {
        if (Contents is not null) return;
        var directories = Directory.GetDirectories(FilePath);
        var files = Directory.GetFiles(FilePath);
        int x = 0;
        Contents = new FileSystemObject[directories.Length + files.Length];
        for(int i = 0; i < directories.Length; i++)
        {
            Contents[x] = Create(directories[i]);
            x++;
        }
        for(int i = 0; i < files.Length; i++)
        {
            Contents[x] = Create(files[i]);
            x++;
        }
    }
    public static FileSystemObject Create(string path)
    {
        return new FileSystemObject(path, FileIcon.GetIcon(path));
    }
}