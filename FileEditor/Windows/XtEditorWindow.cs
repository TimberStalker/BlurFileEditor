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
using Editor.Windows;
using GLib;
using ImGuiNET;
using Pango;

public class XtEditorWindow : GuiWindow
{
    string File { get; }
    public XtDatabase LocalDatabase { get; }
    public XtDatabase GlobalDatabase { get; }
    public IDictionary<uint, string> RecordSourceMappings { get; }
    string Name { get; }

    int changeCount = 0;
    UndoCommandListBuffer commandBuffer = new();
    HistoryQueue<UndoCommand> CommandHistory { get; } = new(128);
    public XtEditorWindow(string file, XtDatabase localDatabase, XtDatabase globalDatabase, IDictionary<uint, string> recordSourceMappings)
    {
        File = file;
        LocalDatabase = localDatabase;
        GlobalDatabase = globalDatabase;
        RecordSourceMappings = recordSourceMappings;
        Name = Path.GetFileName(file);
    }
    public unsafe bool Draw()
    {
        bool open = true;
        if (ImGui.Begin(changeCount != 0 ? $"{Name}*###{File}" : $"{Name}###{File}", ref open, ImGuiWindowFlags.NoCollapse))
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
                DrawXtItem(GlobalDatabase, item, item.XtRef, commandBuffer, true);
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
    public static bool HasContent(XtDatabase xtDb, IXtValue value) => value is XtStructValue ||
            value is XtPointerValue p && p.Value is XtStructValue ||
            value is XtHandleValue h && h.Handle is uint r && xtDb.Refs.TryGetValue(r, out var v) && v.Value is XtStructValue ||
            value is XtArrayValue a && a.Array is not null;
    public static bool DrawHeader(XtDatabase xtDb, IXtValueItem item, XtRef reference, bool enabled)
    {
        string text = item switch
        {
            XtRefItem value => $"[{value.Id}]",
            XtFieldValueItem value => $"{value.Field.TargetType} {value.Field.Name}",
            XtArrayItem value => $"[{value.Index}]",
            _ => "Unknown"
        };
        if(HasContent(xtDb, item.Value))
        {
            if(!enabled) ImGui.EndDisabled();
            var header = ImGui.TreeNodeEx(text, ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowOverlap);

            if (!enabled) ImGui.BeginDisabled();
            return header;
        }
        else
        {
            if (!enabled) ImGui.EndDisabled();
            var header = ImGui.TreeNodeEx(text, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowOverlap);

            if (!enabled) ImGui.BeginDisabled();
            return header;
        }
    }
    public static void DrawXtItem(XtDatabase xtDb, IXtValueItem item, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
    {
        bool showContent = DrawHeader(xtDb, item, reference, enabled);

        if(item is XtArrayItem v)
        {
            if (ImGui.BeginPopupContextItem($"itemContext{item.GetHashCode()}"))
            {
                if (ImGui.MenuItem("Remove"))
                {
                    int index = v.Container.Values.IndexOf(v);
                    commandBuffer.Add((target: v.Container, item: v, index), b => b.target.Values.RemoveAt(index), b => b.target.Values.Insert(b.index, b.item));
                }
                ImGui.EndPopup();
            }
        }
        ImGui.SameLine(0, 10);
        ImGui.Text("=");
        switch(item.Value)
        {
            case XtHandleValue handle:
                if (ImGui.BeginPopupContextItem($"itemContext{item.GetHashCode()}"))
                {
                    ImGui.MenuItem("Original File", false);
                    ImGui.Separator();
                    if (ImGui.MenuItem("Clear", enabled))
                    {
                        commandBuffer.Add(handle, null, handle.Handle, (h, v) => h.Handle = v);
                    }
                    ImGui.EndPopup();
                }
                if (handle.Handle is uint h)
                {
                    ImGui.BeginDisabled();
                    DrawItemValueContent(xtDb, handle, xtDb.Refs[h], commandBuffer, showContent, false);
                    ImGui.EndDisabled();
                }
                else
                {
                    DrawItemValueContent(xtDb, item.Value, reference, commandBuffer, showContent, enabled);
                }
                break;
            case XtPointerValue pointer:
                if (ImGui.BeginPopupContextItem($"itemContext{item.GetHashCode()}"))
                {
                    if (ImGui.MenuItem("Clear", enabled))
                    {
                        commandBuffer.Add(pointer, null, pointer.Value, (h, v) => h.Value = v);
                    }
                    ImGui.EndPopup();
                }
                goto default;
            case XtArrayValue array:
                if (ImGui.BeginPopupContextItem($"itemContext{item.GetHashCode()}"))
                {
                    if (ImGui.MenuItem("Clear", enabled))
                    {
                        commandBuffer.Add(array, null, array.Array, (h, v) => h.Array = v);
                    }
                    ImGui.EndPopup();
                }
                goto default;
            default:
                DrawItemValueContent(xtDb, item.Value, reference, commandBuffer, showContent, enabled);
                break;
        }
    }

    private static void DrawItemValueContent(XtDatabase xtDb, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, bool showContent, bool enabled)
    {
        DrawValue(xtDb, value, reference, commandBuffer, enabled);

        if (showContent)
        {
            DrawContent(xtDb, value, reference, commandBuffer, enabled);
            ImGui.TreePop();
        }
    }

    public static void DrawContent(XtDatabase xtDb, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
    {
        if (value is XtStructValue v) {
            var values = CollectionsMarshal.AsSpan(v.Values);
            for (int i = 0; i < values.Length; i++)
            {
                ImGui.PushID(i);
                DrawXtItem(xtDb, values[i], reference, commandBuffer, enabled);
                ImGui.PopID();
            }
        } else if(value is XtPointerValue p && p.Value is not null)
        {
            DrawContent(xtDb, p.Value, reference, commandBuffer, enabled);
        } 
        else if(value is XtHandleValue h && h.Handle is uint r && xtDb.Refs.TryGetValue(r, out var xtRef) && xtRef.Value is XtStructValue)
        {
            DrawContent(xtDb, xtRef.Value, xtRef, commandBuffer, enabled);
        } 
        else if(value is XtArrayValue a && a.Array is not null)
        {

            var values = CollectionsMarshal.AsSpan(a.Array.Values);
            for(int i = 0; i < values.Length; i++)
            {
                ImGui.PushID(i);
                DrawXtItem(xtDb, values[i], reference, commandBuffer, enabled);
                ImGui.PopID();
            }
            if (ImGui.Button("Append"))
            {
                commandBuffer.Add((target: a.Array, item: new XtArrayItem(a.Array, a.Array.Type.BaseType.CreateValue()), reference), b =>
                {
                    b.target.Values.Add(b.item);
                    if(b.item.Type is not IXtCurryType)
                    {
                        b.reference.RefHeap.Add(b.item.Value);
                    }
                }, b =>
                {
                    b.target.Values.Remove(b.item); 
                    if (b.item.Type is not IXtCurryType)
                    {
                        b.reference.RefHeap.Remove(b.item.Value);
                    }
                });
            }
        }
    }
    public static unsafe void DrawValue(XtDatabase xtDb, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
    {
        ImGui.SameLine(0, 10);
        switch (value)
        {
            case XtAtomValue<bool> v:
            {
                ImGui.SetNextItemWidth(80);
                bool edit = v.Value;
                if (ImGui.Checkbox($"##sbyte{v.GetHashCode()}", ref edit))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<sbyte> v:
            {
                ImGui.SetNextItemWidth(80);
                sbyte edit = v.Value;
                if (ImGui.InputScalar($"##sbyte{v.GetHashCode()}", ImGuiDataType.S8, (nint)(&edit)))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<short> v:
            {
                ImGui.SetNextItemWidth(80);
                short edit = v.Value;
                if (ImGui.InputScalar($"##short{v.GetHashCode()}", ImGuiDataType.S16, (nint)(&edit)))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<int> v:
            {
                ImGui.SetNextItemWidth(80);
                int edit = v.Value;
                if (ImGui.InputScalar($"##int{v.GetHashCode()}", ImGuiDataType.S32, (nint)(&edit)))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<long> v:
            {
                ImGui.SetNextItemWidth(80);
                long edit = v.Value;
                if (ImGui.InputScalar($"##long{v.GetHashCode()}", ImGuiDataType.S64, (nint)(&edit)))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<byte> v:
            {
                ImGui.SetNextItemWidth(80);
                byte edit = v.Value;
                if (ImGui.InputScalar($"##byte{v.GetHashCode()}", ImGuiDataType.U8, (nint)(&edit)))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<ushort> v:
            {
                ImGui.SetNextItemWidth(80);
                ushort edit = v.Value;
                if (ImGui.InputScalar($"##ushort{v.GetHashCode()}", ImGuiDataType.U16, (nint)(&edit)))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<uint> v:
            {
                ImGui.SetNextItemWidth(80);
                uint edit = v.Value;
                if (ImGui.InputScalar($"##uint{v.GetHashCode()}", ImGuiDataType.U32, (nint)(&edit)))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<ulong> v:
            {
                ImGui.SetNextItemWidth(80);
                ulong edit = v.Value;
                if (ImGui.InputScalar($"##ulong{v.GetHashCode()}", ImGuiDataType.U64, (nint)(&edit)))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<float> v:
            {
                ImGui.SetNextItemWidth(80);
                float edit = v.Value;
                if (ImGui.InputFloat($"##float{v.GetHashCode()}", ref edit))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<double> v:
            {
                ImGui.SetNextItemWidth(80);
                double edit = v.Value;
                if (ImGui.InputDouble($"##double{v.GetHashCode()}", ref edit))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                break;
            }
            case XtAtomValue<string> v:
            {
                ImGui.PushItemWidth(460);
                string edit = v.Value;
                if (ImGui.InputText($"##string{v.GetHashCode()}", ref edit, 255))
                {
                    commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                }

                ImGui.PopItemWidth();
                break;
            }
            case XtAtomValue<LocId> v:
            {
                ImGui.PushItemWidth(460);
                uint edit = v.Value;
                if (ImGui.InputScalar($"##locid{v.GetHashCode()}", ImGuiDataType.U32, (nint)(&edit)))
                {
                    commandBuffer.Add(v, (LocId)edit, v.Value, (t, value) => t.Value = value);
                }

                ImGui.PopItemWidth();
                break;
            }
            case XtEnumValue v:
                if(v.Type.IsFlags)
                {
                    ImGui.SetNextItemWidth(80);
                    uint edit = v.Value;
                    if (MultiCombo($"##flags{v.GetHashCode()}", ref edit, v.Type.Labels))
                    {
                        commandBuffer.Add(v, edit, v.Value, (t, value) => t.Value = value);
                    }
                }
                else
                {
                    ImGui.SetNextItemWidth(80);
                    int edit = (int)v.Value;
                    if (ImGui.Combo($"##enum{v.GetHashCode()}", ref edit, string.Join('\0', v.Type.Labels)))
                    {
                        commandBuffer.Add(v, (uint)edit, v.Value, (t, value) => t.Value = value);
                    }
                }
                break;
            case XtStructValue v:

                if (!TypeDrawer.Draw(xtDb, value, reference, commandBuffer))
                {
                    ImGui.SetNextItemWidth(80);
                    ImGui.Text($"({v.Type.Name}){v.GetHashCode()}");
                }
                break;
            case XtPointerValue v:

                if(v.Value is null)
                {
                    if (ImGui.Button("new", new Vector2(80, 0)))
                    {
                        ImGui.OpenPopup("newPop");
                    }
                    if(ImGui.BeginPopup("newPop"))
                    {
                        foreach (var item in xtDb.Types.Where(t => t == v.Type.BaseType || (t is XtStructType st && v.Type.BaseType is XtStructType pt && st.IsOfType(pt))))
                        {
                            if (ImGui.Button(item.Name))
                            {
                                commandBuffer.Add((target: v, item: item.CreateValue(), reference),
                                    static b =>
                                    {
                                        b.target.Value = b.item;
                                        b.reference.RefHeap.Add(b.item);
                                    },
                                    static b =>
                                    {
                                        b.target.Value = null;
                                        b.reference.RefHeap.Remove(b.item);
                                    });
                            }
                        }
                        ImGui.EndPopup();
                    }
                    ImGui.SameLine(0, 5);
                    if (ImGui.Button("use", new Vector2(80, 0)))
                    {
                        ImGui.OpenPopup("usePop");
                    }
                    if (ImGui.BeginPopup("usePop"))
                    {
                        ImGui.BeginChild("usePopScroll", new Vector2(600, 200), ImGuiChildFlags.None, ImGuiWindowFlags.AlwaysAutoResize);
                        foreach (var heapValue in reference.RefHeap.Where(t => t.Type == v.Type.BaseType || (t.Type is XtStructType st && v.Type.BaseType is XtStructType pt && st.IsOfType(pt))))
                        {
                            bool showContent = false;
                            if (HasContent(xtDb, heapValue))
                            {
                                showContent = ImGui.TreeNodeEx($"({heapValue.Type}){heapValue.GetHashCode()}", ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowOverlap);
                            }
                            else
                            {
                                showContent = ImGui.TreeNodeEx($"({heapValue.Type}){heapValue.GetHashCode()}", ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.FramePadding | ImGuiTreeNodeFlags.AllowOverlap);
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("add"))
                            {
                                commandBuffer.Add((target: v, item: heapValue),
                                    b => b.target.Value = b.item,
                                    b => b.target.Value = null);
                            }
                            if(showContent)
                            {
                                DrawContent(xtDb, heapValue, reference, commandBuffer, enabled);
                                ImGui.TreePop();
                            }
                        }
                        ImGui.EndChild();
                        ImGui.EndPopup();
                    }
                }
                else
                {
                    DrawValue(xtDb, v.Value, reference, commandBuffer, enabled);
                }
                break;
            case XtHandleValue v:

                ImGui.SetNextItemWidth(80);
                if(v.Handle is null)
                {
                    if (ImGui.Button("new", new Vector2(80, 0)))
                    {
                        
                    }
                    ImGui.SameLine(0, 5);
                    if (ImGui.Button("use", new Vector2(80, 0)))
                    {

                    }
                }
                else
                {
                    if(xtDb.Refs.TryGetValue(v.Handle.Value, out var xtRef))
                    {
                        ImGui.Text($"({xtRef.Type})[{v.Handle}]");
                    }
                    else
                    {
                        ImGui.Text($"[{v.Handle}] Not Loaded");
                    }
                }
                break;
            case XtArrayValue v:

                ImGui.SetNextItemWidth(80);
                if (v.Array is null)
                {
                    if(ImGui.Button("new", new Vector2(80, 0)))
                    {
                        commandBuffer.Add((target: v, array: new XtArray(v.Type), reference), 
                            b =>
                            {
                                b.target.Array = b.array;
                                reference.RefHeap.Add(b.array);
                            },
                            b =>
                            {
                                b.target.Array = null;
                                reference.RefHeap.Remove(b.array);
                            });
                    }
                    ImGui.SameLine(0, 5);
                    if (ImGui.Button("use", new Vector2(80, 0)))
                    {

                    }
                }
                else
                {
                    ImGui.Text($"({v.Array.Type}){v.Array.GetHashCode()}[{v.Array.Count}]");
                }
                break;
            default:
                ImGui.Text("Not Implemented");
                break;
        }
    }
    static bool MultiCombo(string label, ref uint flags, IEnumerable<string> flagNames)
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
            bool changed = false;
            foreach (var flag in flagNames)
            {
                changed |= ImGui.CheckboxFlags(flag, ref flags, i);
                i <<= 1;
            }
            ImGui.EndCombo();
            return changed;
        }
        return false;
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
