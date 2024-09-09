using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using BlurFileFormats.XtFlask;
using BlurFileFormats.XtFlask.Values;
using Editor.Drawers;
using Editor.Rendering;
using ImGuiNET;
using Pango;
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
        static List<XtEditorWindow> windows = [];

        public static void ExecuteOnMainThread(Action action) => ExecuteOnMainThread(_ => action(), null);
        public static void ExecuteOnMainThread<T>(Action<T> action, T state)
        {
            if(SynchronizationContext.Current != synchronizationContext)
            {
                synchronizationContext.Post(s => action((T)s!), state);
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
                synchronizationContext.Post(s => action(s), state);
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
                IsResizable = false,
            };
            
            Window _window = new Window(_winProps);
            new ImGuiController();

            ImGui.CreateContext();

            _window.OnUpdate += () => {
                /******************************/
                /* IMGUI DOCK-SPACE TEST ONLY */
                /******************************/

                ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
                ImGui.SetNextWindowSize(Window.Instance.GetWindowSize());
                ImGui.BeginMainMenuBar();
                if(ImGui.BeginMenu("File"))
                {
                    if(ImGui.MenuItem("Open Standalone"))
                    {
                        Console.WriteLine("Open Standalone File");
                        FileDialogue.OpenPopup("standaloneOpen");
                    }
                    if(ImGui.MenuItem("Create Project"))
                    {
                        Console.WriteLine("Open Create Project Popup");
                        FileDialogue.OpenPopup("createProjectOpen");
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMainMenuBar();

                if (FileDialogue.OpenFile("standaloneOpen", out string file))
                {
                    Console.WriteLine(file);
                    if(Path.GetExtension(file) == ".bin")
                    {
                        windows.Add(new XtEditorWindow(file));
                    }
                }
                if (FileDialogue.OpenFile("createProjectOpen", out string directory))
                {
                    Console.WriteLine(directory);
                }

                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.5f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.5f);
                ImGui.Begin("DOCK-SPACE TEST", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
                                               ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | 
                                               ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus );
                ImGui.PopStyleVar(2);
                ImGui.DockSpace(ImGui.GetID("Dockspace"));
                
                ImGui.End();

                //ImGui.ShowAboutWindow();
                //ImGui.ShowDemoWindow();
                foreach (var window in windows)
                {
                    window.Draw();
                }
            };
            
            _window.Loop();
        }
        
    }
}
public interface IWindow
{
    void Draw();
}
public class FileTreeWindow : IWindow
{
    public void Draw()
    {
    }
}
public class XtEditorWindow : IWindow
{
    XtDb XtDb { get; }
    string Name { get; }
    public XtEditorWindow(string path)
    {
        XtDb = Flask.Import(path);
        Name = Path.GetFileName(path);
        
    }
    public void Draw()
    {
        DrawXtEditorWindow(XtDb, Name);
    }
    static void DrawXtEditorWindow(XtDb xtDb, string name)
    {
        if (ImGui.Begin(name, ImGuiWindowFlags.NoCollapse))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));
            foreach (var item in xtDb.References)
            {
                var value = item.Value;
                if (value == XtNullValue.Instance)
                {
                    ImGui.TreeNodeEx(item.Id.ToString(), ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowItemOverlap);
                    ImGui.SameLine();
                    ImGui.Text(item.Type.Name);
                    ImGui.SameLine();
                    ImGui.Text("External Data");
                    ImGui.TreePop();
                }
                else
                {
                    bool showContent = (ImGui.TreeNodeEx(item.Id.ToString(), ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowItemOverlap));
                    ImGui.SameLine();
                    ImGui.Text(item.Type.Name);
                    if (showContent)
                    {
                        ShowXtValue(value);
                        ImGui.TreePop();
                    }
                }
            }
            ImGui.PopStyleVar();
            ImGui.End();
        }
    }

    static void ShowXtValue(IXtValue xtValue)
    {
        if (xtValue is XtStructValue s)
        {
            foreach (var item in s.Values)
            {
                DrawField(item);
            }
        }
        else if (xtValue is XtArrayValue a)
        {
            for (int i = 0; i < a.Values.Count; i++)
            {
                var value = a.Values[i];
                DrawArrayItem(i, value);
            }
            ImGui.SetNextItemWidth(200);
            ImGui.Button("Add");
        }
    }

    private static void DrawArrayItem(int i, XtArrayValueItem value)
    {
        ImGuiTreeNodeFlags flags = default;
        bool hasContent = TypeDrawer.HasContent(value.Value) || value.Value is IXtMultiValue;
        if (!hasContent)
        {
            flags = ImGuiTreeNodeFlags.Leaf;
        }

        bool showContent = ImGui.TreeNodeEx($"[{i}]", flags | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowItemOverlap);
        ImGui.SameLine();
        ImGui.Text(value.Type.Name);

        if (!TypeDrawer.DrawTitle(value.Value))
        {
            if (value.Value is XtEnumValue v)
            {
                int c = (int)v.Value;
                ImGui.SameLine();
                ImGui.Combo("##enum", ref c, v.XtType.Labels.ToArray(), v.XtType.Labels.Count);
                v.Value = (uint)c;
            }
        }
        if (showContent)
        {
            if (!TypeDrawer.DrawContent(value.Value))
            {
                if (value.Value is IXtMultiValue)
                {
                    ShowXtValue(value.Value);
                }
            }
            ImGui.TreePop();
        }
    }
    public static bool DrawFieldLabel(XtStructValueItem item, ImGuiTreeNodeFlags? overrideFlags = null)
    {
        var field = item.Field;
        var fieldValue = item.Value;

        bool hasContent = TypeDrawer.HasContent(fieldValue) || fieldValue is IXtMultiValue;

        ImGuiTreeNodeFlags flags = default;
        if (overrideFlags is ImGuiTreeNodeFlags f)
        {
            flags = f;
        }
        else
        {
            if (!hasContent)
            {
                flags = ImGuiTreeNodeFlags.Leaf;
            }
        }
        bool showContent = ImGui.TreeNodeEx(field.Name, flags | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowItemOverlap, field.Type.Name);

        ImGui.SameLine();
        ImGui.Text(field.Name);

        return showContent;
    }
    public static bool DrawField(XtStructValueItem item)
    {
        var field = item.Field;
        var fieldValue = item.Value;

        bool showContent = DrawFieldLabel(item);
        DrawTitleContent(fieldValue);
        if (showContent)
        {
            if (!TypeDrawer.DrawContent(fieldValue))
            {
                if (fieldValue is IXtMultiValue)
                {
                    ShowXtValue(fieldValue);
                }
            }
            ImGui.TreePop();
        }
        return showContent;
    }

    private static void DrawTitleContent(IXtValue fieldValue)
    {
        if (!TypeDrawer.DrawTitle(fieldValue))
        {
            if (fieldValue is XtEnumValue v)
            {
                int c = (int)v.Value;
                ImGui.SameLine();
                ImGui.SetNextItemWidth(160);
                ImGui.Combo("##enum", ref c, v.XtType.Labels.ToArray(), v.XtType.Labels.Count);
                v.Value = (uint)c;
            } else if(fieldValue is XtFlagsValue f)
            {
                uint c = f.Value;
                ImGui.SameLine();
                ImGui.SetNextItemWidth(160);
                MultiCombo("##flags", ref c, f.XtType.Labels);
                f.Value = c;
            }
        }
    }
    static void MultiCombo(string label, ref uint flags, IEnumerable<string> flagNames)
    {
        string text = "";
        if (flags == 0)
        {
            text = "None";
        }
        else
        {
            if (flags > 0)
            {
                uint nameFlags = flags;
                StringBuilder builder = new();
                foreach (var item in flagNames)
                {
                    if (nameFlags == 0) break;
                    if ((nameFlags & 1) == 1)
                    {
                        builder.Append(item);
                        builder.Append(',');
                    }
                    nameFlags >>= 1;
                }
                text = builder.Remove(builder.Length - 1, 1).ToString();
            }
        }
        if (ImGui.BeginCombo(label, text))
        {
            uint i = 1;
            foreach (var flag in flagNames)
            {
                //bool isSet = (flags & 1 << i) > 0;
                ImGui.CheckboxFlags(flag, ref flags, i);
                //int x = 1;
                //if (isSet) x = 0;
                //flags ^= x << i;
                i <<= 1;
            }
            ImGui.EndCombo();
        }
    }
}