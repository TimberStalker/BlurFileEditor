using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using BlurFileFormats.FlaskReflection;
using Editor.Projects;
using Gdk;
using ImGuiNET;
using Pango;
using static XtEditorView;
namespace Editor.Drawers
{
    [RequiresUnreferencedCode("Needs access to reflection to load drawers.")]
    public static class XtDrawer
    {
        static Dictionary<string, IValueDrawer<XtStructValue>> StructDrawers { get; } = [];
        static Dictionary<string, IValueDrawer<XtArrayValue>> ArrayDrawers { get; } = [];


        static bool TryDrawValueCustomDrawer(Project project, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
        {
            switch (value)
            {
                case XtArrayValue a:
                    return ArrayDrawers.TryGetValue(value.Type.Name, out var aDrawer)
                        && aDrawer.DrawValue(project, a, reference, commandBuffer, enabled);
                case XtStructValue s:
                    return StructDrawers.TryGetValue(value.Type.Name, out var sDrawer)
                        && sDrawer.DrawValue(project, s, reference, commandBuffer, enabled);
                default:
                    return false;
            }
        }

        public static bool DrawValueCustomDrawer(Project project, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
        {
            switch (value)
            {
                case XtHandleValue handle:
                    if (handle.Handle is null || !project.Flask.TryGetRef(handle.Handle.Value, out var handleReferecne)) return false;
                    return DrawValueCustomDrawer(project, handleReferecne.Value, reference, commandBuffer, enabled);
                case XtPointerValue pointer:
                    if (pointer.Value is null) return false;
                    return DrawValueCustomDrawer(project, pointer.Value, reference, commandBuffer, enabled);
                case var c:
                    return TryDrawValueCustomDrawer(project, value, reference, commandBuffer, enabled);
            }
        }

        static XtDrawer()
        {
            LoadDrawers();
        }
        private static void LoadDrawers()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsInterface && !t.IsAbstract);
            foreach (var type in types)
            {
                if (!typeof(IValueDrawer<XtStructValue>).IsAssignableFrom(type)
                    && !typeof(IValueDrawer<XtArrayValue>).IsAssignableFrom(type))
                {
                    continue;
                }
                var attrs = type.GetCustomAttributes<DrawAtribute>().ToArray();
                if (attrs.Length == 0) continue;

                object drawerInstance = Activator.CreateInstance(type)!;

                var names = attrs.Select(a => a.TypeName);

                TryAddDrawer(drawerInstance, names, StructDrawers);
                TryAddDrawer(drawerInstance, names, ArrayDrawers);
            }
        }
        private static void TryAddDrawer<T>(object possibleDrawer, IEnumerable<string> names, Dictionary<string, IValueDrawer<T>> current) where T : IXtValue
        {
            if (possibleDrawer is not IValueDrawer<T> drawer) return;
            foreach (var name in names)
            {
                if (current.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Drawer with name {name} already exists.");
                }
                current[name] = drawer;
            }
        }

        public static bool HasContent(Project project, IXtValue value) => value is XtStructValue ||
            value is XtPointerValue p && p.Value is XtStructValue ||
            value is XtHandleValue h && h.Handle is uint r && project.Flask.TryGetRef(r, out var v) && v.Value is XtStructValue ||
            value is XtArrayValue a && a.Array is not null;
        public static bool DrawHeader(Project project, IXtValueItem item, XtRef reference, bool enabled)
        {
            string text = item switch
            {
                XtRefItem value => $"[{value.Id}]",
                XtFieldValueItem value => $"{value.Field.TargetType} {value.Field.Name}",
                XtArrayItem value => $"[{value.Index}]",
                _ => "Unknown"
            };
            if (HasContent(project, item.Value))
            {
                if (!enabled) ImGui.EndDisabled();
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
        public static void DrawXtItem(Project project, IXtValueItem item, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
        {
            bool showContent = DrawHeader(project, item, reference, enabled);
            if (item.Value is XtHandleValue { Handle: uint beginHandle })
            {
                if (ImGui.BeginPopupContextItem($"itemContext{item.GetHashCode()}"))
                {
                    ImGui.MenuItem($"Original File: {project.Flask.GetRecordSourceFile(beginHandle)}", false);
                    ImGui.Separator();
                    ImGui.EndPopup();
                }
            }
            if (item is XtArrayItem v)
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
            switch (item.Value)
            {
                case XtHandleValue handle:
                    if (ImGui.BeginPopupContextItem($"itemContext{item.GetHashCode()}"))
                    {
                        if (ImGui.MenuItem("Clear", enabled))
                        {
                            commandBuffer.Add(handle, null, handle.Handle, (h, v) => h.Handle = v);
                        }
                        ImGui.EndPopup();
                    }
                    if (handle.Handle is uint h)
                    {
                        if(enabled) ImGui.BeginDisabled();
                        if (project.Flask.TryGetRef(h, out var handleReference))
                        {
                            DrawItemValueContent(project, handle, handleReference, commandBuffer, showContent, false);
                        }
                        else
                        {
                            DrawValue(project, handle, reference, commandBuffer, enabled);
                        }
                        if (enabled) ImGui.EndDisabled();
                    }
                    else
                    {
                        DrawItemValueContent(project, item.Value, reference, commandBuffer, showContent, enabled);
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
                    DrawItemValueContent(project, item.Value, reference, commandBuffer, showContent, enabled);
                    break;
            }
        }

        private static void DrawItemValueContent(Project project, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, bool showContent, bool enabled)
        {
            DrawValue(project, value, reference, commandBuffer, enabled);

            if (showContent)
            {
                DrawContent(project, value, reference, commandBuffer, enabled);
                ImGui.TreePop();
            }
        }

        public static void DrawContent(Project project, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
        {
            if (value is XtStructValue v)
            {
                var values = CollectionsMarshal.AsSpan(v.Values);
                for (int i = 0; i < values.Length; i++)
                {
                    ImGui.PushID(i);
                    DrawXtItem(project, values[i], reference, commandBuffer, enabled);
                    ImGui.PopID();
                }
            }
            else if (value is XtPointerValue p && p.Value is not null)
            {
                DrawContent(project, p.Value, reference, commandBuffer, enabled);
            }
            else if (value is XtHandleValue h && h.Handle is uint r && project.Flask.TryGetRef(r, out var xtRef) && xtRef.Value is XtStructValue)
            {
                DrawContent(project, xtRef.Value, xtRef, commandBuffer, enabled);
            }
            else if (value is XtArrayValue a && a.Array is not null)
            {

                var values = CollectionsMarshal.AsSpan(a.Array.Values);
                for (int i = 0; i < values.Length; i++)
                {
                    ImGui.PushID(i);
                    DrawXtItem(project, values[i], reference, commandBuffer, enabled);
                    ImGui.PopID();
                }
                if (ImGui.Button("Append"))
                {
                    commandBuffer.Add((target: a.Array, item: new XtArrayItem(a.Array, a.Array.Type.BaseType.CreateValue()), reference), b =>
                    {
                        b.target.Values.Add(b.item);
                        if (b.item.Type is not IXtCurryType)
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
        public static unsafe void DrawValue(Project project, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
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
                    if (v.Type.IsFlags)
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

                    if (!DrawValueCustomDrawer(project, value, reference, commandBuffer, enabled))
                    {
                        ImGui.SetNextItemWidth(80);
                        ImGui.Text($"({v.Type.Name}){v.GetHashCode()}");
                    }
                    break;
                case XtPointerValue v:

                    if (v.Value is null)
                    {
                        if (ImGui.Button("new", new Vector2(80, 0)))
                        {
                            ImGui.OpenPopup("newPop");
                        }
                        if (ImGui.BeginPopup("newPop"))
                        {
                            foreach (var item in project.Flask.Types.Where(t => t == v.Type.BaseType || (t is XtStructType st && v.Type.BaseType is XtStructType pt && st.IsOfType(pt))))
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
                            if(ImGui.BeginChild("usePopScroll", new Vector2(600, 200)))
                            {
                                foreach (var heapValue in reference.RefHeap.Where(t => t.Type == v.Type.BaseType || (t.Type is XtStructType st && v.Type.BaseType is XtStructType pt && st.IsOfType(pt))))
                                {
                                    bool showContent = false;
                                    if (HasContent(project, heapValue))
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
                                    if (showContent)
                                    {
                                        DrawContent(project, heapValue, reference, commandBuffer, enabled);
                                        ImGui.TreePop();
                                    }
                                }
                                ImGui.EndChild();
                            }
                            ImGui.EndPopup();
                        }
                    }
                    else
                    {
                        DrawValue(project, v.Value, reference, commandBuffer, enabled);
                    }
                    break;
                case XtHandleValue v:

                    ImGui.SetNextItemWidth(80);
                    if (v.Handle is null)
                    {
                        if (ImGui.Button("use", new Vector2(80, 0)))
                        {
                            ImGui.OpenPopup("useHandlePop");
                        }
                    }
                    else
                    {
                        if (project.Flask.TryGetRef(v.Handle.Value, out var xtRef))
                        {
                            ImGui.Text($"({xtRef.Type})[{v.Handle}]");
                        }
                        else
                        {
                            ImGui.Text($"[{v.Handle}] Not Loaded");
                        }
                    }
                    GetNewRecordPopup(project, commandBuffer, v);
                    break;
                case XtArrayValue v:

                    ImGui.SetNextItemWidth(80);
                    if (v.Array is null)
                    {
                        if (ImGui.Button("new", new Vector2(80, 0)))
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

        private static unsafe void GetNewRecordPopup(Project project, ICommandBuffer commandBuffer, XtHandleValue v)
        {
            if (ImGui.BeginPopup("useHandlePop"))
            {
                if (ImGui.BeginListBox("##handleListBox", new Vector2(600, 400)))
                {
                    foreach (var (handle, record) in project.Flask.GlobalXtDatabase.Refs.Where(r => r.Value.Type.IsOfType(v.Type.BaseType)))
                    {
                        ImGui.PushID(handle.GetHashCode());
                        if (ImGui.Button("use"))
                        {
                            commandBuffer.Add(v, handle, v.Handle, (r, v) => r.Handle = v);
                        }
                        ImGui.SameLine();
                        ImGui.BeginDisabled();
                        DrawXtItem(project, new XtRefItem(handle, record), record, commandBuffer, false);
                        ImGui.EndDisabled();
                        ImGui.PopID();
                    }
                    ImGui.EndListBox();
                }
                ImGui.EndPopup();
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
    }

    public interface IValueDrawer<in T> where T : IXtValue
    {
        bool DrawValue(Project project, T value, XtRef reference, ICommandBuffer commandBuffer, bool enabled);
    }
}
