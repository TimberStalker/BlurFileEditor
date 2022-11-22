using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace FileResearcher.Utils;
public static class CursorHelper
{
    [StructLayout(LayoutKind.Sequential)]
    struct Win32Point
    {
        public Int32 X;
        public Int32 Y;
    };

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(ref Win32Point pt);

    public static Point GetCurrentCursorPosition()
    {
        Win32Point w32Mouse = new Win32Point();
        GetCursorPos(ref w32Mouse);
        return new Point(w32Mouse.X, w32Mouse.Y);
    }
    public static Point GetCurrentCursorPositionRelativeTo(Visual relativeTo)
    {
        Win32Point w32Mouse = new Win32Point();
        GetCursorPos(ref w32Mouse);
        return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
    }
}
