using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlurFileFormats.FlaskReflection;
using Editor.Rendering;
using Editor.Windows;
using Editor.Windows.Popups;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Pango;
using static Editor.Rendering.GL;
using ImGuiController = Editor.Rendering.IMGUI.ImGuiController;

namespace Editor
{
    /// <summary>
    /// The entry point of the editor.
    /// </summary>
    static class Program 
    {
        [AllowNull]
        static SynchronizationContext synchronizationContext;
        static GuiWindowManager WindowManager = new();

        public static void ExecuteOnMainThread(Action action) => ExecuteOnMainThread(_ => action(), null);
        public static void ExecuteOnMainThread<T1, T2>(Action<T1, T2> action, T1 state1, T2 state2)
            => ExecuteOnMainThread(c => action(c.state1, c.state2), (state1, state2));
        
        public static void ExecuteOnMainThread<T1, T2, T3>(Action<T1, T2, T3> action, T1 state1, T2 state2, T3 state3)
            => ExecuteOnMainThread(c => action(c.state1, c.state2, c.state3), (state1, state2, state3));

        public static void ExecuteOnMainThread<T>(Action<T> action, T state)
        {
            if(SynchronizationContext.Current != synchronizationContext)
            {
                synchronizationContext.Post(static s =>
                {
                    (Action<T> action, T state) = ((Action<T>, T))s!;
                    action(state);
                }, (action, state));
            }
            else
            {
                action(state);
            }
        }
        public static void ExecuteOnMainThread(Action<object?> action, object state)
        {
            if(SynchronizationContext.Current != synchronizationContext)
            {
                synchronizationContext.Post(static s =>
                {
                    (Action<object?> action, object state) = ((Action<object?>, object))s!;
                    action(state);
                }, (action, state));
            }
            else
            {
                action(state);
            }
        }
        static void Main(string[] _args) {
            SynchronizationContext.SetSynchronizationContext(synchronizationContext = new SynchronizationContext());

            WindowCreationProps _winProps = new WindowCreationProps() {
                Title = "BLUR FILE EDITOR",
                IsResizable = true,
            };
            var serviceCollection = new ServiceCollection();

            Window _window = new Window(_winProps);
            new ImGuiController();
            ImGui.CreateContext();
            _window.OnUpdate += static () => {
                ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
                ImGui.SetNextWindowSize(Window.Instance.WindowSize);
                ImGui.BeginMainMenuBar();
                if(ImGui.BeginMenu("File"))
                {
                    if(ImGui.MenuItem("Open Standalone"))
                    {
                        Console.WriteLine("Open Standalone File");
                        FileDialogue.OpenPopup("standaloneOpen");
                    }
                    ImGui.Separator();
                    if(ImGui.MenuItem("Create Project"))
                    {
                        Console.WriteLine("Create Project Popup");
                        FileDialogue.OpenPopup("createProjectOpen");
                    }
                    if(ImGui.MenuItem("Open Project"))
                    {
                        Console.WriteLine("Open Project Popup");
                        FileDialogue.OpenPopup("openProjectOpen");
                    }
                    ImGui.Separator();
                    if(ImGui.BeginMenu("Open Recent Project", AppSettings.Instance.RecentProjects.Count > 0))
                    {
                        for (int i = 0; i < AppSettings.Instance.RecentProjects.Count; i++)
                        {
                            string? item = AppSettings.Instance.RecentProjects[i];
                            var items = item.Split(Path.DirectorySeparatorChar);
                            if (ImGui.MenuItem(Path.Join(items[^3], items[^2], items[^1])))
                            {
                                var project = Project.Load(item);
                                if(project is not null)
                                {
                                    AppSettings.Instance.RecentProjects.RemoveAll(f => f == item);
                                    AppSettings.Instance.RecentProjects.Insert(0, item);
                                    AppSettings.Instance.Save();

                                    OpenProject(Path.GetDirectoryName(item)!, project);
                                    break;
                                }
                            }
                        }
                        ImGui.EndMenu();
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();

                if (FileDialogue.OpenFile("standaloneOpen", out string file))
                {
                    Console.WriteLine(file);
                    if(Path.GetExtension(file) == ".bin" || Path.GetExtension(file) == ".xt")
                    {
                        var xt = Flask.Import(file);
                        var sourceMappings = new Dictionary<uint, string>();
                        foreach (var (key, _) in xt.Refs)
                        {
                            sourceMappings[key] = file;
                        }
                        //WindowManager.AddWindow(new XtEditorWindow(file, xt, xt, sourceMappings));
                    }
                }
                if (FileDialogue.OpenFile("createProjectOpen", out string directory, "*.exe"))
                {
                    Console.WriteLine(directory);
                    string dir = Path.GetDirectoryName(directory)!;
 

                    string[] files = Directory.GetFiles( dir, "*.*", SearchOption.AllDirectories);
                    var locFiles = files.Where(f => Path.GetExtension(f) == ".loc").Select(f => Path.GetRelativePath(dir, f)).ToArray();
                    var flaskExtensions = new List<string> { ".bin", ".xt" };
                    var flaskFiles = files.Where(f => flaskExtensions.Contains(Path.GetExtension(f))).Select(f => Path.GetRelativePath(dir,f)).ToArray();

                    var project = new Project();

                    project.FlaskFiles.AddRange(flaskFiles);
                    project.LocFiles.AddRange(locFiles);

                    Project.Save(Path.Combine(dir, "editor.blurproj"), project);

                    AppSettings.Instance.RecentProjects.Add(Path.Combine(dir, "editor.blurproj"));
                    AppSettings.Instance.Save();

                    OpenProject(dir, project);
                }
                if (FileDialogue.OpenFile("openProjectOpen", out string projFile, "*.blurproj"))
                {
                    Console.WriteLine(projFile);
                    string dir = Path.GetDirectoryName(projFile)!;
                    var project = Project.Load(projFile);
                    if(project is not null)
                    {
                        AppSettings.Instance.RecentProjects.RemoveAll(f => f == projFile);
                        AppSettings.Instance.RecentProjects.Insert(0, projFile);
                        AppSettings.Instance.Save();

                        OpenProject(dir, project);
                    }
                }

                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.5f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.5f);
                ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
                ImGui.SetNextWindowSize(Window.Instance.WindowSize, ImGuiCond.Always);
                ImGui.Begin("DOCK-SPACE TEST", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
                                               ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove |
                                               ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus );
                ImGui.PopStyleVar(2);
                ImGui.DockSpace(ImGui.GetID("Dockspace"));

                ImGui.End();

                //ImGui.ShowAboutWindow();
                //ImGui.ShowDemoWindow();
                WindowManager.Draw();
            };

            serviceCollection.AddSingleton(_window);
            serviceCollection.AddSingleton(WindowManager);

            var provider = serviceCollection.BuildServiceProvider();

            _window.Loop();
        }
        static XtDatabase GlobalXtDatabase { get; } = new();
        static ConcurrentDictionary<string, XtDatabase> ScopedDatabases { get; } = [];
        static ConcurrentDictionary<uint, string> RecordSourceMappings { get; } = [];
        static string RootDirectory = "";
        static void OpenProject(string directory, Project project)
        {
            WindowManager.Clear();

            RootDirectory = directory;

            ProjectExplorerWindow explorerWindow = new ProjectExplorerWindow(directory);

            GlobalXtDatabase.Refs.Clear();
            GlobalXtDatabase.Types.Clear();

            ScopedDatabases.Clear();

            ConcurrentDictionary<uint, XtRef> globalRefs = [];

            Parallel.ForEach(project.FlaskFiles, item =>
            {
                try
                {
                    var scopedDatabase = Flask.Import(Path.Combine(directory, item));
                    ScopedDatabases[item] = scopedDatabase;
                    foreach (var (key, value) in scopedDatabase.Refs)
                    {
                        globalRefs[key] = value;
                        RecordSourceMappings[key] = item;
                    }
                } catch(Exception ex)
                {
                    Console.WriteLine($"Failed to parse {item}");
                    Console.WriteLine(ex.ToString());
                }
            });

            foreach (var (key, item) in globalRefs)
            {
                GlobalXtDatabase.Refs[key] = item;
            }

            explorerWindow.OnOpenFile += (_, f) => OpenFile(f);
            WindowManager.AddWindow(explorerWindow);
        }
        static void OpenFile(string file)
        {
            if (!File.Exists(file)) return;
            if (MatchesExtension(file, ".bin", ".xt"))
            {
                var localFile = Path.GetRelativePath(RootDirectory, file);
                WindowManager.AddWindow(new XtEditorWindow(file, ScopedDatabases[localFile], GlobalXtDatabase, RecordSourceMappings));
            }
            else if (MatchesExtension(file, ".png", ".jpg"))
            {
                WindowManager.AddWindow(new ImageWindow(file));
            }
            else if (MatchesExtension(file, ".dds"))
            {
                WindowManager.AddWindow(new DirectXImageWindow(file));
            }
        }
        static bool MatchesExtension(string file, params string[] extensions)
        {
            string extension = Path.GetExtension(file);
            return extensions.Any(e => e == extension);
        }
    }
}

class UndoCommandListBuffer : List<UndoCommand>, ICommandBuffer
{
    public void Add<T>(in T value, Action<T> redo, Action<T> undo) where T : notnull
    {
        Add(UndoCommand.Create(value, redo, undo));
    }
}
public interface ICommandBuffer
{
    void Add<T>(in T value, Action<T> redo, Action<T> undo) where T : notnull;
}

public static class CommandBufferExtensions
{
    record struct ValueChanged<TTarget, TValue>(TTarget target, TValue oldValue, TValue newValue, Action<TTarget, TValue> action);
    public static void Add<TTarget, TValue>(this ICommandBuffer commandBuffer, TTarget target, TValue newValue, TValue oldValue, Action<TTarget, TValue> action)
    {
        commandBuffer.Add(new ValueChanged<TTarget, TValue>(target, oldValue, newValue, action),
            static v => v.action(v.target, v.newValue),
            static v => v.action(v.target, v.oldValue));
    }
}
