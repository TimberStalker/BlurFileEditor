using BlurFileFormats.XtFlask.Values;

namespace Editor.Drawers
{
    public interface ITypeContentDrawer : ITypeDrawer
    {
        public void DrawContent(IXtValue xtValue);
    }
}