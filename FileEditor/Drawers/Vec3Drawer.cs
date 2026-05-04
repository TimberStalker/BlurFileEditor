using BlurFileFormats.FlaskReflection;
using Editor.Projects;
using GLib;
using Hexa.NET.ImGui;
using System.Diagnostics.CodeAnalysis;

namespace Editor.Drawers
{
    [DrawAtribute("Vec3")]
    [RequiresUnreferencedCode("")]
    public class Vec3Drawer : IValueDrawer<XtStructValue>
    {
        public bool DrawValue(Project project, XtStructValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
        {
            var xItem = value.GetFieldItem("vx");
            var yItem = value.GetFieldItem("vy");
            var zItem = value.GetFieldItem("vz");

            ImGui.Text("(");

            ImGui.SameLine(0, 0);
            XtDrawer.DrawValue(project, xItem.Value, reference, commandBuffer, enabled);

            ImGui.SameLine(0, 0);
            ImGui.Text(",");

            ImGui.SameLine(0, 0);
            XtDrawer.DrawValue(project, yItem.Value, reference, commandBuffer, enabled);

            ImGui.SameLine(0, 0);
            ImGui.Text(",");

            ImGui.SameLine(0, 0);
            XtDrawer.DrawValue(project, zItem.Value, reference, commandBuffer, enabled);

            ImGui.SameLine();
            ImGui.Text(")");
            return true;
        }
    }
    [DrawAtribute("Vec2")]
    [RequiresUnreferencedCode("")]
    public class Vec2Drawer : IValueDrawer<XtStructValue>
    {
        public bool DrawValue(Project project, XtStructValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
        {
            var xItem = value.GetFieldItem("vx");
            var yItem = value.GetFieldItem("vy");

            ImGui.Text("(");

            ImGui.SameLine(0, 0);
            XtDrawer.DrawValue(project, xItem.Value, reference, commandBuffer, enabled);

            ImGui.SameLine(0, 0);
            ImGui.Text(",");

            ImGui.SameLine(0, 0);
            XtDrawer.DrawValue(project, yItem.Value, reference, commandBuffer, enabled);

            ImGui.SameLine();
            ImGui.Text(")");
            return true;
        }
    }
    [DrawAtribute("RangeI8")]
    [RequiresUnreferencedCode("")]
    public class RangeI8Drawer : IValueDrawer<XtStructValue>
    {
        public bool DrawValue(Project project, XtStructValue value, XtRef reference, ICommandBuffer commandBuffer, bool enabled)
        {
            var fromItem = value.GetFieldItem("from");
            var toItem = value.GetFieldItem("to");

            XtDrawer.DrawValue(project, fromItem.Value, reference, commandBuffer, enabled);

            ImGui.SameLine(0, 0);
            ImGui.Text("-");

            ImGui.SameLine(0, 0);
            XtDrawer.DrawValue(project, toItem.Value, reference, commandBuffer, enabled);
            return true;
        }
    }
}