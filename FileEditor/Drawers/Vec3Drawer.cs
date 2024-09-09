using BlurFileFormats.XtFlask.Values;
using ImGuiNET;

namespace Editor.Drawers
{
    [DrawAtribute("Vec3")]
    public class Vec3Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtStructValue s)
            {
                var xAtom = (XtAtomValue<float>)s.GetField("vx");
                var yAtom = (XtAtomValue<float>)s.GetField("vy");
                var zAtom = (XtAtomValue<float>)s.GetField("vz");

                float x = xAtom.Value;
                float y = yAtom.Value;
                float z = zAtom.Value;
                ImGui.PushItemWidth(40);

                ImGui.SameLine(ImGui.GetColumnOffset(), 20);
                ImGui.InputFloat("X", ref x);
                ImGui.SameLine();
                ImGui.InputFloat("Y", ref y);
                ImGui.SameLine();
                ImGui.InputFloat("Z", ref z);

                ImGui.PopItemWidth();

                xAtom.Value = x;
                yAtom.Value = y;
                zAtom.Value = z;
            }
        }
    }
}