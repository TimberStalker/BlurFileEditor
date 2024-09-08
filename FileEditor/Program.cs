using System;
using System.Numerics;
using Editor.Rendering;
using ImGuiNET;
using ImGuiController = Editor.Rendering.IMGUI.ImGuiController;

namespace Editor {

    /// <summary>
    /// The entry point of the editor.
    /// </summary>
    static class Program {

        static void Main(string[] _args) {
            WindowCreationProps _winProps = new WindowCreationProps() {
                Title = "BLUR FILE EDITOR",
                IsResizable = false,
            };
            
            Window _window = new Window(_winProps);
            new ImGuiController();
            
            _window.OnUpdate += () => {
                /******************************/
                /* IMGUI DOCK-SPACE TEST ONLY */
                /******************************/
                ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
                ImGui.SetNextWindowSize(Window.Instance.GetWindowSize());
                ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.5f);
                ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.5f);
                ImGui.Begin("DOCK-SPACE TEST", ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
                                               ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | 
                                               ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus );
                ImGui.PopStyleVar(2);
                ImGui.DockSpace(ImGui.GetID("Dockspace"));
                
                ImGui.End();
                
                ImGui.ShowAboutWindow();
                ImGui.ShowDemoWindow();
            };
            
            _window.Loop();
        }
        
    }
    
}