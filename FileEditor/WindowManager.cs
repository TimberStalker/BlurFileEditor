using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor;
public class WindowManager
{
    List<IWindow> windows = [];
    List<IWindow> pendingWindows = [];
    public void AddWindow(IWindow window)
    {
        pendingWindows.Add(window);
    }
    public void RemoveWindow(IWindow window)
    {
        pendingWindows.Remove(window);
    }
    public void Draw()
    {
        IWindow? removeWindow = null;
        foreach(var pendingWindow in pendingWindows)
        {
            windows.Add(pendingWindow);
        }
        pendingWindows.Clear();
        foreach (var window in windows)
        {
            if (!window.Draw())
            {
                removeWindow = window;
            }
        }
        if (removeWindow is not null)
        {
            windows.Remove(removeWindow);
        }
    }
}
