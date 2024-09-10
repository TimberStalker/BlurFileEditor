using BlurFileFormats.XtFlask.Values;
using ImGuiNET;

namespace Editor.Drawers
{
    [DrawAtribute("Vec3")]
    public class Vec3Drawer : ITypeTitleDrawer
    {
        public void Draw(BlurFileFormats.XtFlask.XtDb xtDb, IXtValue xtValue)
        {
            if (xtValue is XtStructValue s)
            {
                var xAtom = (XtAtomValue<float>)s.GetField("vx");
                var yAtom = (XtAtomValue<float>)s.GetField("vy");
                var zAtom = (XtAtomValue<float>)s.GetField("vz");

                float x = xAtom.Value;
                float y = yAtom.Value;
                float z = zAtom.Value;
                ImGui.PushItemWidth(60);

                ImGui.SameLine();
                ImGui.Text("(");

                ImGui.SameLine();
                ImGui.InputFloat("##X", ref x);

                ImGui.SameLine();
                ImGui.Text(",");

                ImGui.SameLine();
                ImGui.InputFloat("##Y", ref y);

                ImGui.SameLine();
                ImGui.Text(",");

                ImGui.SameLine();
                ImGui.InputFloat("##Z", ref z);

                ImGui.SameLine();
                ImGui.Text(")");

                ImGui.PopItemWidth();

                xAtom.Value = x;
                yAtom.Value = y;
                zAtom.Value = z;
            }
        }
    }
    [DrawAtribute("Vec2")]
    public class Vec2Drawer : ITypeTitleDrawer
    {
        public void Draw(BlurFileFormats.XtFlask.XtDb xtDb, IXtValue xtValue)
        {
            if (xtValue is XtStructValue s)
            {
                var xAtom = (XtAtomValue<float>)s.GetField("vx");
                var yAtom = (XtAtomValue<float>)s.GetField("vy");

                float x = xAtom.Value;
                float y = yAtom.Value;
                ImGui.PushItemWidth(60);

                ImGui.SameLine();
                ImGui.Text("(");

                ImGui.SameLine();
                ImGui.InputFloat("##X", ref x);

                ImGui.SameLine();
                ImGui.Text(",");

                ImGui.SameLine();
                ImGui.InputFloat("##Y", ref y);

                ImGui.SameLine();
                ImGui.Text(")");

                ImGui.PopItemWidth();

                xAtom.Value = x;
                yAtom.Value = y;
            }
        }
    }
    [DrawAtribute("RangeI8")]
    public class RangeI8Drawer : ITypeTitleDrawer
    {
        public unsafe void Draw(BlurFileFormats.XtFlask.XtDb xtDb, IXtValue xtValue)
        {
            if (xtValue is XtStructValue s)
            {
                var fromAtom = (XtAtomValue<sbyte>)s.GetField("from");
                var toAtom = (XtAtomValue<sbyte>)s.GetField("to");

                sbyte from = fromAtom.Value;
                sbyte to = toAtom.Value;
                ImGui.PushItemWidth(60);

                ImGui.SameLine();
                ImGui.InputScalar("##from", ImGuiDataType.S8, (nint)(&from));

                ImGui.SameLine();
                ImGui.Text("-");

                ImGui.SameLine();
                ImGui.InputScalar("##to", ImGuiDataType.S8, (nint)(&to));

                ImGui.PopItemWidth();

                fromAtom.Value = from;
                toAtom.Value = to;
            }
        }
    }
}