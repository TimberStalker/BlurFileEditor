using BlurFileFormats.FlaskReflection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
namespace Editor.Drawers
{
    [RequiresUnreferencedCode("Needs access to reflection to load drawers.")]
    public static class TypeDrawer
    {
        static Dictionary<string, IValueDrawer<XtStructValue>> StructDrawers { get; } = [];
        static Dictionary<string, IValueDrawer<XtArrayValue>> ArrayDrawers { get; } = [];


        static bool TryDraw(XtDatabase xtDb, IXtValue value, XtRef reference, ICommandBuffer commandBuffer)
        {
            switch (value)
            {
                case XtArrayValue a:
                    return ArrayDrawers.TryGetValue(value.Type.Name, out var aDrawer)
                        && aDrawer.DrawValue(xtDb, a, reference, commandBuffer, true);
                case XtStructValue s:
                    return StructDrawers.TryGetValue(value.Type.Name, out var sDrawer)
                        && sDrawer.DrawValue(xtDb, s, reference, commandBuffer, true);
                default:
                    return false;
            }
        }

        public static bool Draw(XtDatabase xtDb, IXtValue value, XtRef reference, ICommandBuffer commandBuffer)
        {
            switch (value)
            {
                case XtHandleValue handle:
                    if (handle.Handle is null) return false;
                    return Draw(xtDb, xtDb.Refs[handle.Handle.Value].Value, reference, commandBuffer);
                case XtPointerValue pointer:
                    if (pointer.Value is null) return false;
                    return Draw(xtDb, pointer.Value, reference, commandBuffer);
                case var c:
                    return TryDraw(xtDb, value, reference, commandBuffer);
            }
        }

        static TypeDrawer()
        {
            LoadDrawers();
        }
        private static void LoadDrawers()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsInterface && !t.IsAbstract);
            foreach (var type in types)
            {
                if (!typeof(IValueDrawer<XtStructValue>).IsAssignableFrom(type)
                    && !typeof(IValueDrawer<XtArrayValue>).IsAssignableFrom(type))
                {
                    continue;
                }
                var attrs = type.GetCustomAttributes<DrawAtribute>().ToArray();
                if (attrs.Length == 0) continue;

                object drawerInstance = Activator.CreateInstance(type)!;

                var names = attrs.Select(a => a.TypeName);

                TryAddDrawer(drawerInstance, names, StructDrawers);
                TryAddDrawer(drawerInstance, names, ArrayDrawers);
            }
        }
        private static void TryAddDrawer<T>(object possibleDrawer, IEnumerable<string> names, Dictionary<string, IValueDrawer<T>> current) where T : IXtValue
        {
            if (possibleDrawer is not IValueDrawer<T> drawer) return;
            foreach (var name in names)
            {
                if (current.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Drawer with name {name} already exists.");
                }
                current[name] = drawer;
            }
        }
    }

    public interface IValueDrawer<in T> where T : IXtValue
    {
        bool DrawValue(XtDatabase xtDb, T value, XtRef reference, ICommandBuffer commandBuffer, bool enabled);
    }
}
