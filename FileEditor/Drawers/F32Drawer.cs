using BlurFileFormats.XtFlask.Values;
using ImGuiNET;

namespace Editor.Drawers
{
    [DrawAtribute("f32")]
    public class F32Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<float> a)
            {
                float v = a.Value;

                ImGui.PushItemWidth(60);

                ImGui.SameLine(ImGui.GetColumnOffset(), 20);
                ImGui.InputFloat("##float", ref v);

                ImGui.PopItemWidth();

                a.Value = v;
            }
        }
    }
    [DrawAtribute("f64")]
    public class F64Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<double> a)
            {
                double v = a.Value;

                ImGui.PushItemWidth(60);

                ImGui.SameLine(ImGui.GetColumnOffset(), 20);
                ImGui.InputDouble("##double", ref v);
                ImGui.PopItemWidth();

                a.Value = v;
            }
        }
    }
}