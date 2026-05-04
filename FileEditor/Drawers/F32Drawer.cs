using BlurFileFormats.FlaskReflection;
using Editor.Projects;
using Hexa.NET.ImGui;
using System;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Editor.Drawers
{
    [DrawAtribute("RP_LinearFunction")]
    public class LinearFunctionDrawer : IValueDrawer<XtStructValue>
    {
        public bool DrawValue(Project project, XtStructValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
        {
            var yScale = value.GetField<float>("yScale");
            var dataPoints = value.GetFieldItem("dataPoints");
            if (dataPoints.Value is not XtArrayValue { Array: XtArray array})
            {
                return false;
            }
            int i = 0;
            float[] fPoints = new float[array.Count];
            foreach (var item in array)
            {
                if (item.Value is XtAtomValue<float> f)
                {
                    fPoints[i] = f.Value * yScale.Value;
                }
                i++;
            }
            var available = ImGui.GetContentRegionAvail();
            ImGui.PlotLines("##Graph", ref fPoints[0], i, 0, "", 0, 1, new System.Numerics.Vector2(available.X - 20, 0));
            if (ImGui.IsItemClicked())
            {
                ImGui.OpenPopup("linerFunctionPopup");
            }
            if (ImGui.BeginPopup("linerFunctionPopup"))
            {
                if(ImGui.BeginChild("linerFunctionPopupScroll", new Vector2(600, 200)))
                {

                ImGui.EndChild();
                }


                ImGui.EndPopup();
            }
            return true;
        }
        static void DrawPlot()
        {
            //ImGui.GetWindowDrawList().add
        }
        static void RenderFrame(Vector2 p_min, Vector2 p_max, uint fill_col, bool borders, float rounding)
        {
            ImGui.NewFrame();
            var windowDrawList = ImGui.GetWindowDrawList();
            windowDrawList.AddRectFilled(p_min, p_max, fill_col, rounding);
            float border_size = ImGui.GetStyle().FrameBorderSize;
            if (borders && border_size > 0.0f)
            {
                windowDrawList.AddRect(p_min + new Vector2(1, 1), p_max + new Vector2(1, 1), ImGui.GetColorU32(ImGuiCol.BorderShadow), rounding, 0, border_size);
                windowDrawList.AddRect(p_min, p_max, ImGui.GetColorU32(ImGuiCol.Border), rounding, 0, border_size);
            }
        }
    }
    //[DrawAtribute("Render_Curve")]
    //public class CurveDrawer : ITypeContentDrawer
    //{
    //    public void DrawContent(XtDb xtDb, IXtValue xtValue)
    //    {
    //        ;
    //    }
    //}
}