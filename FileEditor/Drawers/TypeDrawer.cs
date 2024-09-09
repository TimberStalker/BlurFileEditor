using System.Reflection;
using BlurFileFormats.XtFlask.Types;
using BlurFileFormats.XtFlask.Values;

namespace Editor.Drawers
{
    public static class TypeDrawer
    {
        static Dictionary<string, ITypeDrawer> Drawers { get; } = [];

        public static bool HasDrawer(IXtValue value) => value is XtNullValue ? false : HasDrawer(value.Type);
        public static bool HasDrawer(IXtType type) => HasDrawer(type.Name);
        public static bool HasDrawer(string type) => Drawers.ContainsKey(type);

        public static bool HasTitle(IXtValue value) => value is XtNullValue ? false : HasTitle(value.Type);
        public static bool HasTitle(IXtType type) => HasTitle(type.Name);
        public static bool HasTitle(string type) => HasDrawer(type) && GetDrawer(type) is ITypeTitleDrawer;

        public static bool HasContent(IXtValue value) => value is XtNullValue ? false : HasContent(value.Type);
        public static bool HasContent(IXtType type) => HasContent(type.Name);
        public static bool HasContent(string type) => HasDrawer(type) && GetDrawer(type) is ITypeContentDrawer;

        public static ITypeDrawer? GetDrawer(IXtType type) => GetDrawer(type.Name);
        public static ITypeDrawer? GetDrawer(string type) => Drawers.TryGetValue(type, out var drawer) ? drawer : null;

        public static bool DrawTitle(IXtValue value)
        {
            if (value is XtNullValue) return false;
            var drawer = GetDrawer(value.Type);
            if (drawer is not ITypeTitleDrawer d) return false;
            d.Draw(value);
            return true;
        }
        public static bool DrawContent(IXtValue value)
        {
            if (value is XtNullValue) return false;
            var drawer = GetDrawer(value.Type);
            if (drawer is not ITypeContentDrawer d) return false;

            d.DrawContent(value);

            return true;
        }

        static TypeDrawer()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsInterface && !t.IsAbstract && t.IsAssignableTo(typeof(ITypeDrawer)));
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<DrawAtribute>();
                if (attr is null) return;
                Drawers[attr.TypeName] = (ITypeTitleDrawer)Activator.CreateInstance(type)!;
            }
        }
    }
}