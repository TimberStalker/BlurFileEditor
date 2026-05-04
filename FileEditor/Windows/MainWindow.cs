using BlurFileFormats.Audio;
using BlurFileFormats.Models;
using BlurFileFormats.Shaders;
using Editor.Panels;
using Editor.Projects;
using Editor.Views;
using Editor.Windows.Popups;
using Hexa.NET.ImGui;
using Hexa.NET.OpenGL;
using System.Numerics;

namespace Editor.Windows;
public class MainWindow
{
    readonly ProjectExplorerWindow projectExplorerWindow = new();
    readonly Dictionary<string, IDynamicView> dynamicViews = [];
    readonly List<IView> staticViews;
    readonly List<string> deleteWindowBuffer = [];
    private readonly GL gl;
    Project? activeProject;
    public MainWindow(GL gl)
    {
        staticViews = [projectExplorerWindow];
        projectExplorerWindow.OnOpenFile += (sender, file) =>
        {
            if (activeProject is null) return;
            OpenFile(activeProject, file);
        };
        this.gl = gl;
    }
    public void DrawMainMenu()
    {
        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Open Standalone"))
            {
                Console.WriteLine("Open Standalone File");
                FileDialogue.OpenPopup("standaloneOpen");
            }
            ImGui.Separator();
            if (ImGui.MenuItem("Create Project"))
            {
                Console.WriteLine("Create Project Popup");
                FileDialogue.OpenPopup("createProjectOpen");
            }
            if (ImGui.MenuItem("Open Project"))
            {
                Console.WriteLine("Open Project Popup");
                FileDialogue.OpenPopup("openProjectOpen");
            }
            ImGui.Separator();
            if (ImGui.BeginMenu("Open Recent Project", AppSettings.Instance.RecentProjects.Count > 0))
            {
                for (int i = 0; i < AppSettings.Instance.RecentProjects.Count; i++)
                {
                    string item = AppSettings.Instance.RecentProjects[i];
                    var items = item.Split(Path.DirectorySeparatorChar);
                    if (ImGui.MenuItem(Path.Join(items[^3], items[^2], items[^1])))
                    {
                        try
                        {
                            activeProject = Project.Open(item);
                            {
                                AppSettings.Instance.RecentProjects.RemoveAll(f => f == item);
                                AppSettings.Instance.RecentProjects.Insert(0, item);
                                AppSettings.Instance.Save();
                            }
                            projectExplorerWindow.SetDirectory(Path.GetDirectoryName(item));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to open project {item}: {ex}");
                        }
                    }
                }
                ImGui.EndMenu();
            }
            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("Edit"))
        {

            ImGui.EndMenu();
        }
        if (ImGui.BeginMenu("View"))
        {
            DrawViewMenuItem(projectExplorerWindow);
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();
    }
    public void Draw(Vector2 windowSize)
    {
        DrawMainMenu();
        //ImGui.SetNextWindowSize(windowSize);
        if (FileDialogue.OpenFile("standaloneOpen", out string file))
        {
            Console.WriteLine(file);
            Project project = Project.CreateEmpty();
            OpenFile(project, file);
        }
        if (FileDialogue.OpenFile("createProjectOpen", out string directory, "*.exe"))
        {
            Console.WriteLine(directory);
            string dir = Path.GetDirectoryName(directory)!;

            activeProject = Project.Open(dir);

            AppSettings.Instance.RecentProjects.Add(Path.Combine(dir, "editor.blurproj"));
            AppSettings.Instance.Save();
            projectExplorerWindow.SetDirectory(dir);
        }
        if (FileDialogue.OpenFile("openProjectOpen", out string projFile, "*.blurproj"))
        {
            Console.WriteLine(projFile);
            string dir = Path.GetDirectoryName(projFile)!;
            activeProject = Project.Open(projFile);
             
            AppSettings.Instance.RecentProjects.RemoveAll(f => f == projFile);
            AppSettings.Instance.RecentProjects.Insert(0, projFile);
            AppSettings.Instance.Save();
            projectExplorerWindow.SetDirectory(dir);
        }
        //ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.5f);
        //ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.5f);
        //ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
        //ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);
        //ImGui.Begin("DOCK-SPACE TEST", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
        //                               ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
        //                               ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus);
        //ImGui.PopStyleVar(2);
        //ImGui.DockSpace(ImGui.GetID("Dockspace"));

        //ImGui.End();

        foreach (var item in staticViews)
        {
            item.Draw();
        }
        foreach (var (id, item) in dynamicViews)
        {
            if(!item.Draw(gl))
            {
                deleteWindowBuffer.Add(id);
            }
        }
        foreach (var item in deleteWindowBuffer)
        {
            dynamicViews.Remove(item);
        }
        deleteWindowBuffer.Clear();
    }
    void AddDynamicView(IDynamicView view)
    {
        if (dynamicViews.ContainsKey(view.Id)) return;
        dynamicViews.Add(view.Id, view);
    }
    async void OpenFile(Project project, string filePath)
    {
        try
        {
            switch(Path.GetExtension(filePath))
            {
                case ".bin":
                case ".xt":
                    var xtDatabase = await project.Flask.GetXtDatabase(filePath);
                    if (xtDatabase is null) return;
                    XtEditorView xtEditor = new(filePath, xtDatabase, project);
                    AddDynamicView(xtEditor);
                    break;
                case ".baf":
                    var baf = Baf.Parse(filePath);
                    BafEditorView bafEditor = new(filePath, baf);
                    AddDynamicView(bafEditor);
                    break;
                case ".model":
                    var model = CPModelSerializer.Import(filePath);
                    ModelView modelView = new(gl, model, activeProject!, filePath);
                    AddDynamicView(modelView);
                    break;
                case ".fxb":
                    var fxb = FXBSerializer.Import(filePath);
                    FXBView fxbView = new(gl, fxb, filePath);
                    AddDynamicView(fxbView);
                    break;
            }
        } catch(Exception ex)
        {
            Console.WriteLine($"Failed to open file {filePath}: {ex}");
        }
    }

    void DrawViewMenuItem(IView item)
    {
        if(ImGui.MenuItem(item.Name, item.Shortcut))
        {
            item.Focus();
        }
    }
}
public interface IEditCommands
{
    void DrawEditMenu();
}