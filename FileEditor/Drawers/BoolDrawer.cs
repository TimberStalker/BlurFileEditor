using BlurFileFormats.XtFlask.Values;
using ImGuiNET;

namespace Editor.Drawers
{
    [DrawAtribute("bool")]
    public class BoolDrawer : ITypeTitleDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<bool> a)
            {
                bool v = a.Value;


                ImGui.SameLine(ImGui.GetColumnOffset(), 20);
                ImGui.Checkbox("##check", ref v);


                a.Value = v;
            }
        }
    }
}