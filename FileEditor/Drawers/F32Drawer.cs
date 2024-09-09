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
    [DrawAtribute("Rgb")]
    public class RgbDrawer : ITypeTitleDrawer, ITypeContentDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtStructValue s)
            {
                var rValue = (XtAtomValue<float>)s.GetField("r");
                var gValue = (XtAtomValue<float>)s.GetField("g");
                var bValue = (XtAtomValue<float>)s.GetField("b");

                var color = new System.Numerics.Vector3(rValue.Value, gValue.Value, bValue.Value);

                ImGui.SameLine();
                ImGui.ColorEdit3($"##color {xtValue.GetHashCode()}", ref color, ImGuiColorEditFlags.NoInputs);

                rValue.Value = color.X;
                gValue.Value = color.Y;
                bValue.Value = color.Z;
            }
        }

        public void DrawContent(IXtValue xtValue)
        {
            if (xtValue is XtStructValue s)
            {
                var rItem = s.GetFieldItem("r");
                var gItem = s.GetFieldItem("g");
                var bItem = s.GetFieldItem("b");

                ImGui.PushItemWidth(80);

                if (XtEditorWindow.DrawFieldLabel(rItem))
                {
                    var rValue = (XtAtomValue<float>)rItem.Value;
                    float r = rValue.Value;

                    ImGui.SameLine();
                    ImGui.DragFloat("##r", ref r, 0.001f, 0, 1);

                    rValue.Value = r;
                    ImGui.TreePop();
                }
                if (XtEditorWindow.DrawFieldLabel(gItem))
                {
                    var gValue = (XtAtomValue<float>)gItem.Value;
                    float g = gValue.Value;

                    ImGui.SameLine();
                    ImGui.DragFloat("##g", ref g, 0.001f, 0, 1);

                    gValue.Value = g;
                    ImGui.TreePop();
                }
                if (XtEditorWindow.DrawFieldLabel(bItem))
                {
                    var bValue = (XtAtomValue<float>)bItem.Value;
                    float b = bValue.Value;

                    ImGui.SameLine();
                    ImGui.DragFloat("##b", ref b, 0.001f, 0, 1);

                    bValue.Value = b;
                    ImGui.TreePop();
                }

                ImGui.PopItemWidth();
            }
        }
    }
    [DrawAtribute("Rgba")]
    public class RgbaDrawer : ITypeTitleDrawer, ITypeContentDrawer
    {
        public void Draw(IXtValue xtValue)
        {
            if (xtValue is XtStructValue s)
            {
                var rValue = (XtAtomValue<float>)s.GetField("r");
                var gValue = (XtAtomValue<float>)s.GetField("g");
                var bValue = (XtAtomValue<float>)s.GetField("b");
                var aValue = (XtAtomValue<float>)s.GetField("a");

                var color = new System.Numerics.Vector4(rValue.Value, gValue.Value, bValue.Value, aValue.Value);

                ImGui.SameLine();

                ImGui.ColorEdit4($"##color {xtValue.GetHashCode()}", ref color, ImGuiColorEditFlags.NoInputs);

                rValue.Value = color.X;
                gValue.Value = color.Y;
                bValue.Value = color.Z;
                aValue.Value = color.W;
            }
        }

        public void DrawContent(IXtValue xtValue)
        {
            if (xtValue is XtStructValue s)
            {
                var rItem = s.GetFieldItem("r");
                var gItem = s.GetFieldItem("g");
                var bItem = s.GetFieldItem("b");
                var aItem = s.GetFieldItem("a");

                ImGui.PushItemWidth(80);

                if (XtEditorWindow.DrawFieldLabel(rItem))
                {
                    var rValue = (XtAtomValue<float>)rItem.Value;
                    float r = rValue.Value;

                    ImGui.SameLine();
                    ImGui.DragFloat("##r", ref r, 0.001f, 0, 1);

                    rValue.Value = r;
                    ImGui.TreePop();
                }
                if (XtEditorWindow.DrawFieldLabel(gItem))
                {
                    var gValue = (XtAtomValue<float>)gItem.Value;
                    float g = gValue.Value;

                    ImGui.SameLine();
                    ImGui.DragFloat("##g", ref g, 0.001f, 0, 1);

                    gValue.Value = g;
                    ImGui.TreePop();
                }
                if (XtEditorWindow.DrawFieldLabel(bItem))
                {
                    var bValue = (XtAtomValue<float>)bItem.Value;
                    float b = bValue.Value;

                    ImGui.SameLine();
                    ImGui.DragFloat("##b", ref b, 0.001f, 0, 1);

                    bValue.Value = b;
                    ImGui.TreePop();
                }
                if (XtEditorWindow.DrawFieldLabel(aItem))
                {
                    var aValue = (XtAtomValue<float>)aItem.Value;
                    float a = aValue.Value;

                    ImGui.SameLine();
                    ImGui.DragFloat("##a", ref a, 0.001f, 0, 1);

                    aValue.Value = a;
                    ImGui.TreePop();
                }

                ImGui.PopItemWidth();
            }
        }
    }
}