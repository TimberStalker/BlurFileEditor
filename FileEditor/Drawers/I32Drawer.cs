using BlurFileFormats.XtFlask.Values;
using ImGuiNET;

namespace Editor.Drawers
{
    [DrawAtribute("i64")]
    public class I64Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<long> a)
            {
                long v = a.Value;

                ImGui.SameLine(0, 20);
                ImGui.SetNextItemWidth(80);
                unsafe
                {
                    ImGui.InputScalar("##long", ImGuiDataType.S64, (nint)(&v));
                }

                a.Value = v;
            }
        }
    }
    [DrawAtribute("i32")]
    public class I32Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<int> a)
            {
                int v = a.Value;


                ImGui.SameLine(0, 20);
                ImGui.SetNextItemWidth(80);
                unsafe
                {
                    ImGui.InputScalar("##int", ImGuiDataType.S32, (nint)(&v));
                }

                a.Value = v;
            }
        }
    }
    [DrawAtribute("i16")]
    public class I16Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<short> a)
            {
                short v = a.Value;


                ImGui.SameLine(0, 20);
                ImGui.SetNextItemWidth(80);
                unsafe
                {
                    ImGui.InputScalar("##short", ImGuiDataType.S16, (nint)(&v));
                }

                a.Value = v;
            }
        }
    }
    [DrawAtribute("i8")]
    public class I8Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<sbyte> a)
            {
                sbyte v = a.Value;


                ImGui.SameLine(0, 20);
                ImGui.SetNextItemWidth(80);
                unsafe
                {
                    ImGui.InputScalar("##byte", ImGuiDataType.S8, (nint)(&v));
                }

                a.Value = v;
            }
        }
    }

    [DrawAtribute("u64")]
    public class U64Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<ulong> a)
            {
                ulong v = a.Value;

                ImGui.SameLine(0, 20);
                ImGui.SetNextItemWidth(80);
                unsafe
                {
                    ImGui.InputScalar("##ulong", ImGuiDataType.U64, (nint)(&v));
                }

                a.Value = v;
            }
        }
    }
    [DrawAtribute("u32")]
    public class U32Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<uint> a)
            {
                uint v = a.Value;


                ImGui.SameLine(0, 20);
                ImGui.SetNextItemWidth(80);
                unsafe
                {
                    ImGui.InputScalar("##uint", ImGuiDataType.U32, (nint)(&v));
                }

                a.Value = v;
            }
        }
    }
    [DrawAtribute("u16")]
    public class U16Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<ushort> a)
            {
                ushort v = a.Value;


                ImGui.SameLine(0, 20);
                ImGui.SetNextItemWidth(80);
                unsafe
                {
                    ImGui.InputScalar("##short", ImGuiDataType.U16, (nint)(&v));
                }

                a.Value = v;
            }
        }
    }
    [DrawAtribute("u8")]
    public class U8Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<byte> a)
            {
                byte v = a.Value;


                ImGui.SameLine(0, 20);
                ImGui.SetNextItemWidth(80);
                unsafe
                {
                    ImGui.InputScalar("##byte", ImGuiDataType.U8, (nint)(&v));
                }

                a.Value = v;
            }
        }
    }
}