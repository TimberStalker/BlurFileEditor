namespace Editor.Drawers
{
    public class DrawAtribute : Attribute
    {
        public string TypeName { get; }
        public DrawAtribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}