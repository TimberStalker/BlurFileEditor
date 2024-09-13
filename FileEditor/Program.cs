using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using BlurFileFormats.XtFlask;
using BlurFileFormats.XtFlask.Types;
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
        public static void ExecuteOnMainThread<T1, T2>(Action<T1, T2> action, T1 state1, T2 state2)
            => ExecuteOnMainThread(c => action(c.state1, c.state2), (state1, state2));
        
        public static void ExecuteOnMainThread<T1, T2, T3>(Action<T1, T2, T3> action, T1 state1, T2 state2, T3 state3)
            => ExecuteOnMainThread(c => action(c.state1, c.state2, c.state3), (state1, state2, state3));

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
                IsResizable = true,
            };
            Window _window = new Window(_winProps);
            new ImGuiController();
            ImGui.CreateContext();

            _window.OnUpdate += () => {
                /******************************/
                /* IMGUI DOCK-SPACE TEST ONLY */
                /******************************/

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
                    if(Path.GetExtension(file) == ".bin" || Path.GetExtension(file) == ".xt")
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
                Action? removeWindows = null;
                foreach (var window in windows)
                {
                    if(!window.Draw())
                    {
                        removeWindows += () => windows.Remove(window);
                    }
                }
                removeWindows?.Invoke();
            };
            
            _window.Loop();
        }
        
    }
}
public interface IWindow
{
    bool Draw();
}
public class FileTreeWindow : IWindow
{
    public bool Draw()
    {
        return true;
    }
}
public class XtEditorWindow : IWindow
{
    XtDb XtDb { get; }
    string File { get; }
    string Name { get; }
    public XtEditorWindow(string path)
    {
        XtDb = Flask.Import(path);
        File = path;
        Name = Path.GetFileName(path);
        
    }
    public bool Draw()
    {
        return DrawXtEditorWindow(XtDb, File, Name);
    }
    static bool DrawXtEditorWindow(XtDb xtDb, string file, string name)
    {
        bool open = true;
        if (ImGui.Begin($"{name}##{file}", ref open, ImGuiWindowFlags.NoCollapse))
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
                        ShowXtValue(xtDb, value);
                        ImGui.TreePop();
                    }
                }
            }
            ImGui.PopStyleVar();
            ImGui.End();
        }
        return open;
    }

    static void ShowXtValue(XtDb xtDb, IXtValue xtValue)
    {
        if (xtValue is XtStructValue s)
        {
            foreach (var item in s.Values)
            {
                DrawField(xtDb, item);
            }
        }
        else if (xtValue is XtArrayValue a)
        {
            for (int i = 0; i < a.Values.Count; i++)
            {
                var value = a.Values[i];
                DrawArrayItem(xtDb, i, value);
            }
            ImGui.Indent();
            if(ImGui.Button("Add", new Vector2(180, 0)))
            {
                a.Values.Add(new XtArrayValueItem(a.Type.ElementType.CreateDefault()));
            }
            ImGui.Unindent();
        }
    }
    private static bool DrawArrayItem(XtDb xtDb, int i, XtArrayValueItem value)
    {
        ImGuiTreeNodeFlags flags = default;
        bool hasContent = TypeDrawer.HasContent(value.Value);
        if (!hasContent)
        {
            flags = ImGuiTreeNodeFlags.Leaf;
        }

        bool showContent = ImGui.TreeNodeEx($"[{i}]", flags | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowItemOverlap);
        ImGui.SameLine();
        ImGui.Text(value.Type.Name);

        DrawTitleContent(xtDb, value.Value);
        if (showContent)
        {
            DrawContent(xtDb, value.Value);
            ImGui.TreePop();
        }
        return showContent;
    }
    public static bool DrawField(XtDb xtDb, XtStructValueItem item)
    {
        var field = item.Field;
        var fieldValue = item.Value;

        bool showContent = DrawFieldLabel(item);
        DrawTitleContent(xtDb, fieldValue);
        if (showContent)
        {
            DrawContent(xtDb, fieldValue);
            ImGui.TreePop();
        }
        return showContent;
    }

    public static bool DrawFieldLabel(XtStructValueItem item, ImGuiTreeNodeFlags? overrideFlags = null)
    {
        var field = item.Field;
        var fieldValue = item.Value;

        bool hasContent = TypeDrawer.HasContent(fieldValue);

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

    private static void DrawTitleContent(XtDb xtDb, IXtValue value)
    {
        if(value is IXtReferenceValue refVal)
        {
            value = refVal.Reference;
        }
        if (!TypeDrawer.DrawTitle(xtDb, value))
        {
            if (value is XtEnumValue v)
            {
                int c = (int)v.Value;
                ImGui.SameLine();
                ImGui.SetNextItemWidth(160);
                ImGui.Combo("##enum", ref c, v.XtType.Labels.ToArray(), v.XtType.Labels.Count);
                v.Value = (uint)c;
            } else if(value is XtFlagsValue f)
            {
                uint c = f.Value;
                ImGui.SameLine();
                ImGui.SetNextItemWidth(160);
                MultiCombo("##flags", ref c, f.XtType.Labels);
                f.Value = c;
            } else if(value is XtNullValue)
            {
                ImGui.SameLine();
                ImGui.Text("null");
            }
        }
    }
    public static void DrawContent(XtDb xtDb, IXtValue value)
    {
        if (value is IXtReferenceValue refVal)
        {
            DrawContent(xtDb, refVal.Reference);
            return;
        }
        if (!TypeDrawer.DrawContent(xtDb, value))
        {
            if (value is IXtMultiValue)
            {
                ShowXtValue(xtDb, value);
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