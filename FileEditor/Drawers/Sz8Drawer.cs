using BlurFileFormats.XtFlask.Values;
using ImGuiNET;

namespace Editor.Drawers
{
    [DrawAtribute("sz8")]
    public class Sz8Drawer : ITypeTitleDrawer
    {
        public void Draw(BlurFileFormats.XtFlask.XtDb xtDb, IXtValue xtValue)
        {
            if (xtValue is XtAtomValue<string> a)
            {
                string v = a.Value;

                ImGui.PushItemWidth(460);

                ImGui.SameLine(ImGui.GetColumnOffset(), 20);
                ImGui.InputText($"##string {xtValue.GetHashCode()}", ref v, 255);

                ImGui.PopItemWidth();

                a.Value = v;
            }
        }
    }
}