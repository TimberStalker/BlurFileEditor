using BlurFileFormats.XtFlask.Values;
using ImGuiNET;

namespace Editor.Drawers
{
    [DrawAtribute("i32")]
    public class I32Drawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<int> a)
            {
                int v = a.Value;

                ImGui.PushItemWidth(80);

                ImGui.SameLine(ImGui.GetColumnOffset(), 20);
                ImGui.InputInt("##int", ref v);

                ImGui.PopItemWidth();

                a.Value = v;
            }
        }
    }
}