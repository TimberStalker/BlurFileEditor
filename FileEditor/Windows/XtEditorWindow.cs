using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using BlurFileFormats.FlaskReflection;
using Editor;
using Editor.Drawers;
using Editor.Projects;
using Editor.Views;
using Editor.Windows;
using GLib;
using ImGuiNET;
using Pango;

public class XtEditorWindow : IDynamicView
{
    public string File { get; }
    public XtDatabase LocalDatabase { get; }
    public Project Project { get; }
    public string Name { get; }




    int changeCount = 0;
    UndoCommandListBuffer commandBuffer = new();
    HistoryQueue<UndoCommand> CommandHistory { get; } = new(128);

    string IDynamicView.Id => File;

    string? IDynamicView.Shortcut => null;

    public XtEditorWindow(string file, XtDatabase localDatabase, Project project)
    {
        File = file;
        LocalDatabase = localDatabase;
        Project = project;
        Name = Path.GetFileName(file);
    }
    public bool Draw()
    {
        bool open = true;
        ImGuiWindowFlags flags = ImGuiWindowFlags.NoCollapse;
        if(changeCount != 0)
        {
            flags |= ImGuiWindowFlags.UnsavedDocument;
        }
        if (ImGui.Begin($"{Name}###{File}", ref open, flags))
        {
            if(ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
            {
                if(ImGui.IsKeyDown(ImGuiKey.ModCtrl))
                {
                    if (ImGui.IsKeyReleased(ImGuiKey.S))
                    {
                        if(changeCount > 0)
                        {
                            Flask.Export(LocalDatabase, File);
                            changeCount = 0;
                        }
                    }
                    if (ImGui.IsKeyReleased(ImGuiKey.Z))
                    {
                        if (ImGui.IsKeyDown(ImGuiKey.ModShift))
                        {
                            if (CommandHistory.TryConsume(out var command))
                            {
                                command.Do();
                                changeCount++;
                            }
                        }
                        else
                        {
                            if (CommandHistory.TryPop(out var command))
                            {
                                command.Undo();
                                changeCount--;
                            }
                        }
                    }
                }
            }
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));

            int size = LocalDatabase.Refs.Count;
            XtRefItem[] refs = ArrayPool<XtRefItem>.Shared.Rent(size);

            int index = 0;
            foreach (var (id, xtRef) in LocalDatabase.Refs)
            {
                refs[index++] = new XtRefItem(id, xtRef);
            }

            for (int i = 0; i < size; i++)
            {
                XtRefItem item = refs[i];
                ImGui.PushID(i);
                XtDrawer.DrawXtItem(Project, item, item.XtRef, commandBuffer, true);
                ImGui.PopID();
            }

            ArrayPool<XtRefItem>.Shared.Return(refs);

            ImGui.PopStyleVar();

            if (commandBuffer.Count > 0)
            {
                for (int i = 0; i < commandBuffer.Count; i++)
                {
                    commandBuffer[i].Do();
                    CommandHistory.Push(commandBuffer[i]);
                }
                changeCount += commandBuffer.Count;
                commandBuffer.Clear();
            }

            ImGui.End();
        }
        return open;
    }

    public class XtRefItem : IXtValueItem
    {
        public XtRefItem(object id, XtRef xtRef)
        {
            Id = id;
            XtRef = xtRef;
        }

        public IXtType Type => XtRef.Type;
        public object Id { get; }
        public XtRef XtRef { get; }
        object IXtValueItem.Key => Id;
        IXtValue IXtValueItem.Value
        {
            get => XtRef.Value;
            set => XtRef.Value = value;
        }
    }
}
public struct UndoCommand
{
    readonly object target;
    readonly Action<object> redo;
    readonly Action<object> undo;

    public UndoCommand(object target, Action<object> redo, Action<object> undo)
    {
        this.target = target;
        this.redo = redo;
        this.undo = undo;
    }

    public void Do() => redo(target);
    public void Undo() => undo(target);

    public static UndoCommand Create<T>(T target, Action<T> redo, Action<T> undo) where T : notnull
    {
        return new UndoCommand((target, redo, undo), t =>
        {
            var (target, redo, _) = ((T, Action<T>, Action<T>))t;
            redo.Invoke(target);

        }, t =>
        {
            var (target, _, undo) = ((T, Action<T>, Action<T>))t;
            undo.Invoke(target);
        });
    }
}
public class HistoryQueue<T>
{
    T[] items;
    int start;
    int end;
    int current;
    readonly int capacity;
    public int Capacity => capacity;
    public HistoryQueue(int capacity)
    {
        items = new T[capacity];
        this.capacity = capacity;
    }
    public void Push(in T value)
    {
        items[current] = value;
        current = mod(current + 1, capacity);
        end = current;

        if(end == start)
        {
            start = mod(start + 1, capacity);
        }
    }
    public void PushEnd(in T value)
    {
        items[end] = value;
        end = mod(end + 1, capacity);

        if(end == start)
        {
            start = mod(start + 1, capacity);
        }
    }
    public bool TryPeek([NotNullWhen(true)] out T? value)
    {
        if(current == start)
        {
            value = default;
            return false;
        }
        value = items[mod(current - 1, capacity)]!;
        return true;
    }
    public bool TryPop([NotNullWhen(true)] out T? value)
    {
        if (current == start)
        {
            value = default;
            return false;
        }
        current = mod(current - 1, capacity);
        value = items[current]!;
        return true;
    }
    public bool TryConsume([NotNullWhen(true)] out T? value)
    {
        if (current == end)
        {
            value = default;
            return false;
        }
        value = items[current]!;
        current = mod(current + 1, capacity);
        return true;
    }
    static int mod(int a, int b)
    {
        return a - b * (a / b);
    }
}
