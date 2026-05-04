using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Editor.Panels.Views;
using Hexa.NET.ImGui;

namespace Editor.Panels;
public class Panel
{
    public string Name { get; }
    public string Id { get; }
    public Editor.Panels.Views.IView View { get; }

    public Panel(string name, string id, IView view)
    {
        Name = name;
        Id = id;
        View = view;
    }

    public Panel(string name, IView view) : this(name, name, view)
    {
    }

    bool open;
    bool focus;
    public void Draw()
    {
        if(focus)
        {
            ImGui.SetNextWindowFocus();
        }
        var flags = ImGuiWindowFlags.NoCollapse;
        if(View.Changed)
        {
            flags |= ImGuiWindowFlags.UnsavedDocument;
        }
        if (ImGui.Begin($"{Name}##{Id}", ref open, flags))
        {
            if(ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows))
            {
                View.HandleKeybinds();
            }
            View.Draw();
        }
            ImGui.End();
        if(View.Changed)
        {
            open = true;
        }
    }
    public void Focus()
    {
        open = true;
        focus = true;
    }
}
