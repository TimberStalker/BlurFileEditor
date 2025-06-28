using BlurFileFormats.FlaskReflection;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
#if DEBUG
[assembly: System.Reflection.Metadata.MetadataUpdateHandlerAttribute(typeof(Editor.Drawers.HotReloadService))]
namespace Editor.Drawers
{
    public static class HotReloadService
    {
        public static event Action<Type[]?>? UpdateApplicationEvent;

        internal static void ClearCache(Type[]? types) { }
        internal static void UpdateApplication(Type[]? types)
        {
            UpdateApplicationEvent?.Invoke(types);
        }
    }
}
#endif
namespace Editor.Drawers
{
    public static class TypeDrawer
    {
        static Dictionary<string, (object, MethodInfo drawMethod)> Drawers { get; } = [];

        public static bool HasDrawer(IXtValue value) => HasDrawer(value.Type);
        public static bool HasDrawer(IXtType type) => HasDrawer(type.Name);
        public static bool HasDrawer(string type) => Drawers.ContainsKey(type);


        public static (object, MethodInfo drawMethod) GetDrawer(IXtType type) => GetDrawer(type.Name);
        public static (object, MethodInfo drawMethod) GetDrawer(string type) => Drawers[type];

        public static bool Draw(XtDatabase xtDb, IXtValue value, XtRef reference, ICommandBuffer commandBuffer)
        {
            if (!HasDrawer(value)) return false;
            var (drawer, drawMethod) = GetDrawer(value.Type);
            return Draw(xtDb, value, reference, commandBuffer, drawer, drawMethod);
        }

        private static bool Draw(XtDatabase xtDb, IXtValue value, XtRef reference, ICommandBuffer commandBuffer, object drawer, MethodInfo drawMethod)
        {
            switch(value)
            {
                case XtHandleValue handle:
                    if (handle.Handle is null) return false;
                    return Draw(xtDb, xtDb.Refs[handle.Handle.Value].Value, reference, commandBuffer, drawer, drawMethod);
                case XtPointerValue pointer:
                    if (pointer.Value is null) return false;
                    return Draw(xtDb, pointer.Value, reference, commandBuffer, drawer, drawMethod);
                case var c:
                    var result = drawMethod.Invoke(drawer, [xtDb, value, reference, commandBuffer]);
                    if (result is bool b) return b;
                    return true;
            }
        }

        static TypeDrawer()
        {
#if (DEBUG)
            HotReloadService.UpdateApplicationEvent += _ => LoadDrawers();
#endif
            LoadDrawers();
        }
        private static void LoadDrawers()
        {
            Drawers.Clear();
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(t => !t.IsInterface && !t.IsAbstract);
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<DrawAtribute>();
                if (attr is null) continue;
                Drawers[attr.TypeName] = (Activator.CreateInstance(type)!, type.GetMethod("DrawValue"));
            }
        }
    }
}
