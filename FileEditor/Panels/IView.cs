using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.Views;
internal interface IView
{
    string Name { get; }
    string Id { get; }
    string? Shortcut { get; }
    
    void Draw();
    void Focus();
}
internal interface IDynamicView
{
    string Name { get; }
    string Id { get; }
    string? Shortcut { get; }
    
    bool Draw();
}
