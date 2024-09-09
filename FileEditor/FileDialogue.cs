using System.Numerics;
using Editor.Rendering;
using Gtk;
using ImGuiNET;
using Window = Editor.Rendering.Window;

namespace Editor
{
    public static class FileDialogue
    {
        static Dictionary<string, FileDialogueState> Storage { get; } = [];
        struct FileDialogueState
        {
            public bool open;
            public string path = "C:\\";
            public string editPath = "C:\\";
            public string selectedFile = "";
            public bool directoriesDirty = true;
            public List<FileData> fileSystemEntries { get; } = [];
            public FileDialogueState()
            {
                
            }
        }
        public static void OpenPopup(string id)
        {
            if (!Storage.TryGetValue(id, out var state))
            {
                state = new FileDialogueState();
            }

            state.open = true;
            Storage[id] = state;
        }
        public static bool OpenDirectory(string id, out string directory)
        {
            directory = "";
            bool result = false;
            if (!Storage.TryGetValue(id, out var state))
            {
                state = new FileDialogueState();
            }
            if(state.open)
            {
                ImGui.OpenPopup(id);
                state.open = false;
            }
            ImGui.SetNextWindowSize(new Vector2(700, 400));
            if (ImGui.BeginPopup(id))
            {
                if (state.directoriesDirty)
                {
                    ResetDirectories(ref state, true);
                    state.directoriesDirty = false;
                }
                if (ImGui.ArrowButton("back", ImGuiDir.Left))
                {

                }
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetWindowContentRegionWidth() - ImGui.GetColumnOffset());
                if (ImGui.InputText("##path", ref state.editPath, 255, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (Path.EndsInDirectorySeparator(state.editPath) && Path.Exists(state.editPath))
                    {
                        state.path = state.editPath;
                        state.directoriesDirty = true;
                    }
                    else
                    {
                        state.editPath = state.path;
                    }
                }
                ImGui.BeginChild("fileDisplay", new Vector2(ImGui.GetWindowContentRegionWidth(), ImGui.GetWindowHeight() - 85));


                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));

                ImGui.Columns(4);
                ImGui.Text("Name");
                ImGui.NextColumn();
                ImGui.Text("Date Modified");
                ImGui.NextColumn();
                ImGui.Text("Type");
                ImGui.NextColumn();
                ImGui.Text("Size");
                ImGui.NextColumn();

                foreach (var item in state.fileSystemEntries)
                {
                    var info = item.SystemInfo;
                    if (item.Texture is not null)
                    {
                        ImGui.Image(item.Texture, new Vector2(20, 20));
                        ImGui.SameLine();
                    }
                    else
                    {
                    }
                    if (ImGui.Selectable(info.Name, state.selectedFile == info.Name, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap))
                    {
                        state.selectedFile = info.Name;
                    }
                    if(ImGui.IsItemHovered())
                    {
                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            switch (info)
                            {
                                case DirectoryInfo d:
                                    state.path = Path.Combine(state.path, d.Name) + "\\";
                                    state.editPath = state.path;
                                    state.directoriesDirty = true;
                                    break;
                            }
                        }
                        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            switch (info)
                            {
                                case DirectoryInfo d:
                                    state.selectedFile = d.Name;
                                    break;
                            }
                        }
                    }

                    ImGui.NextColumn();
                    ImGui.Text(info.LastWriteTime.ToString());
                    ImGui.NextColumn();
                    ImGui.Text("Type");
                    ImGui.NextColumn();
                    ImGui.Text("Size");
                    ImGui.NextColumn();
                }
                ImGui.Columns(1);

                ImGui.PopStyleVar();

                ImGui.EndChild();

                ImGui.InputText("##file", ref state.selectedFile, 255);
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    directory = Path.Combine(state.path, state.selectedFile);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
            Storage[id] = state;
            return result;
        }
        public static bool OpenFile(string id, out string file)
        {
            file = "";
            bool result = false;
            if (!Storage.TryGetValue(id, out var state))
            {
                state = new FileDialogueState();
            }
            if(state.open)
            {
                ImGui.OpenPopup(id);
                state.open = false;
            }
            ImGui.SetNextWindowSize(new Vector2(700, 600), ImGuiCond.Always);
            if (ImGui.BeginPopup(id, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (state.directoriesDirty)
                {
                    ResetDirectories(ref state);
                    state.directoriesDirty = false;
                }
                if (ImGui.ArrowButton("back", ImGuiDir.Left))
                {

                }
                ImGui.SameLine();
                ImGui.SetNextItemWidth(ImGui.GetWindowContentRegionWidth() - ImGui.GetColumnOffset());
                if (ImGui.InputText("##path", ref state.editPath, 255, ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    if (Path.EndsInDirectorySeparator(state.editPath) && Path.Exists(state.editPath))
                    {
                        state.path = state.editPath;
                        state.directoriesDirty = true;
                    }
                    else
                    {
                        state.editPath = state.path;
                    }
                }
                ImGui.BeginChild("fileDisplay", new Vector2(ImGui.GetWindowContentRegionWidth(), ImGui.GetWindowHeight() - 85));


                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(5, 5));

                ImGui.Columns(4);
                ImGui.Text("Name");
                ImGui.NextColumn();
                ImGui.Text("Date Modified");
                ImGui.NextColumn();
                ImGui.Text("Type");
                ImGui.NextColumn();
                ImGui.Text("Size");
                ImGui.NextColumn();

                foreach (var item in state.fileSystemEntries)
                {
                    var info = item.SystemInfo;
                    if (item.Texture is not null)
                    {
                        ImGui.Image(item.Texture, new Vector2(20, 20));
                        ImGui.SameLine();
                    }
                    else
                    {
                    }
                    if (ImGui.Selectable(info.Name, state.selectedFile == info.Name, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap))
                    {
                        state.selectedFile = info.Name;
                    }
                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        switch (info)
                        {
                            case DirectoryInfo d:
                                state.path = Path.Combine(state.path, d.Name) + "\\";
                                state.editPath = state.path;
                                state.directoriesDirty = true;
                                break;
                            case FileInfo f:
                                state.selectedFile = info.Name;
                                result = true;
                                file = Path.Combine(state.path, state.selectedFile);
                                ImGui.CloseCurrentPopup();
                                break;
                        }
                    }

                    ImGui.NextColumn();
                    ImGui.Text(info.LastWriteTime.ToString());
                    ImGui.NextColumn();
                    ImGui.Text("Type");
                    ImGui.NextColumn();
                    ImGui.Text("Size");
                    ImGui.NextColumn();
                }
                ImGui.Columns(1);

                ImGui.PopStyleVar();

                ImGui.EndChild();

                ImGui.InputText("##file", ref state.selectedFile, 255);
                ImGui.SameLine();
                if (ImGui.Button("Open"))
                {
                    result = true;
                    file = Path.Combine(state.path, state.selectedFile);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
            Storage[id] = state;
            return result;
        }
        static void ResetDirectories(ref FileDialogueState state, bool exludeFiles = false, string filter = "")
        {
            state.fileSystemEntries.Clear();

            foreach (var directory in Directory.GetDirectories(state.path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                state.fileSystemEntries.Add(new FileData(directoryInfo));
            }
            if(!exludeFiles)
            {
                foreach(var file in Directory.GetFiles(state.path, filter))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    state.fileSystemEntries.Add(new FileData(fileInfo));
                }
            }
        }
        class FileData
        {
            public FileSystemInfo SystemInfo { get; set; }
            public Texture2D? Texture { get; }
            public FileData(FileSystemInfo systemInfo)
            {
                SystemInfo = systemInfo; 
                try
                {
                    var bitmap = FileIcon.GetIcon(SystemInfo.FullName);

                    var texture = new Texture2D();

                    texture.SetParameter(GL.GL_TEXTURE_WRAP_S, GL.GL_CLAMP_TO_EDGE);
                    texture.SetParameter(GL.GL_TEXTURE_WRAP_T, GL.GL_CLAMP_TO_EDGE);
                    texture.SetParameter(GL.GL_TEXTURE_MIN_FILTER, GL.GL_LINEAR);
                    texture.SetParameter(GL.GL_TEXTURE_MAG_FILTER, GL.GL_LINEAR);
                    
                    texture.SetBits(bitmap);
                    Texture = texture;
                }
                catch { }
            }
        }
    }
}
