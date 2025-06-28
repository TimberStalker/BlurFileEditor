using BlurFileFormats.FlaskReflection;
using ImGuiNET;

namespace Editor.Drawers
{
    [DrawAtribute("Rgba")]
    public class RgbaDrawer
    {
        public void DrawValue(XtDatabase xtDb, XtStructValue value, XtRef reference, ICommandBuffer commandBuffer)
        {
            var rValue = value.GetField<float>("r");
            var gValue = value.GetField<float>("g");
            var bValue = value.GetField<float>("b");
            var aValue = value.GetField<float>("a");

            var color = new System.Numerics.Vector4(
                rValue.Value, 
                gValue.Value, 
                bValue.Value,
                aValue.Value);

            ImGui.SameLine();
            if(ImGui.ColorEdit4($"##color {value.GetHashCode()}", ref color, ImGuiColorEditFlags.NoInputs))
            {
                commandBuffer.Add((target: (rValue, gValue, bValue, aValue),
                    oldValues: (r: rValue.Value, g: gValue.Value, b: bValue.Value, a: aValue.Value),
                    newValues: (r: color.X, g: color.Y, b: color.Z, a: color.W)),
                    t => (t.target.rValue.Value, t.target.gValue.Value, t.target.bValue.Value, t.target.aValue.Value) = t.newValues,
                    t => (t.target.rValue.Value, t.target.gValue.Value, t.target.bValue.Value, t.target.aValue.Value) = t.oldValues);
            }
        }
    }
    [DrawAtribute("Route")]
    public class RouteDrawer
    {
        public bool DrawValue(XtDatabase xtDb, XtStructValue value, XtRef reference, ICommandBuffer commandBuffer)
        {
            var name = value.GetField<string>("Name");

            ImGui.Text($"(Route) {name.Value}");
            return true;
        }
    }
    [DrawAtribute("City")]
    public class CityDrawer
    {
        public bool DrawValue(XtDatabase xtDb, XtStructValue value, XtRef reference, ICommandBuffer commandBuffer)
        {
            var name = value.GetField<string>("Name");

            ImGui.Text($"(City) {name.Value}");
            return true;
        }
    }
}