using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using Editor.Rendering;
using ImGuiNET;

namespace Editor
{
    public static class FileDialogue
    {
        static Dictionary<string, FileDialogueState> Storage { get; } = [];
        struct FileDialogueState
        {
            public bool open;
            string path = "";
            public string editPath = "";
            public string[] pathComponents = [];
            public int pathHistoryIndex;
            public List<string> pathHistory = [];
            public string selectedFile = "";
            public bool directoriesDirty;
            public bool textEdit;
            public string Path
            {
                get => pathHistoryIndex == 0 ? path : pathHistory[^(pathHistoryIndex)];
                set {
                    if (value == path) return;
                    while(pathHistoryIndex != 0)
                    {
                        pathHistory.RemoveAt(pathHistory.Count - 1);
                        pathHistoryIndex--;
                    }
                    pathHistory.Add(path);
                    path = value;
                    directoriesDirty = true;
                    editPath = path;
                    pathComponents = path.Split(System.IO.Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                }
            }

            public List<FileData> fileSystemEntries { get; } = [];
            public FileDialogueState()
            {
                Path = Environment.CurrentDirectory;
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
                DirectoryPath(ref state);

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
                                    state.Path = Path.Combine(state.Path, d.Name) + Path.DirectorySeparatorChar;
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
                    directory = Path.Combine(state.Path, state.selectedFile);
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
                DirectoryPath(ref state);

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
                                state.Path = Path.Combine(state.Path, d.Name) + Path.DirectorySeparatorChar;
                                break;
                            case FileInfo f:
                                state.selectedFile = info.Name;
                                result = true;
                                file = Path.Combine(state.Path, state.selectedFile);
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
                    file = Path.Combine(state.Path, state.selectedFile);
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
            Storage[id] = state;
            return result;
        }

        private static void DirectoryPath(ref FileDialogueState state)
        {
            ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
            if(state.textEdit)
            {
                bool returned = ImGui.InputText("##path", ref state.editPath, 255, ImGuiInputTextFlags.EnterReturnsTrue);
                if (returned || (!ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left)))
                {
                    if (Path.EndsInDirectorySeparator(state.editPath) && Path.Exists(state.editPath))
                    {
                        state.Path = state.editPath;
                    }
                    else
                    {
                        state.editPath = state.Path;
                    }
                    state.textEdit = false;
                }
            }
            else
            {
                ImGui.BeginGroup();

                int i = 0;
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(1, 0));
                foreach (var item in state.pathComponents)
                {
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
                    ImGui.ArrowButton($"arrow {i}", ImGuiDir.Right);
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    if (ImGui.Button(item))
                    {
                        if (i < state.pathComponents.Count() - 1)
                        {
                            state.Path = Path.Join(state.pathComponents[0..(i + 1)]) + Path.DirectorySeparatorChar;
                        }
                    }
                    i++;
                }
                ImGui.SameLine();
                if (ImGui.InvisibleButton("switch_to_text", new Vector2(ImGui.GetColumnWidth(), ImGui.GetFrameHeight())))
                {
                    Console.WriteLine("Switch to text entry");
                    state.textEdit = true;
                }
                ImGui.PopStyleVar();
                ImGui.EndGroup();
                unsafe
                {
                    ImGui.GetWindowDrawList().AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), 0xFF7A7A7A);
                }
            }

        }

        static void ResetDirectories(ref FileDialogueState state, bool exludeFiles = false, string filter = "")
        {
            state.fileSystemEntries.Clear();

            foreach (var directory in Directory.GetDirectories(state.Path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                state.fileSystemEntries.Add(new FileData(directoryInfo));
            }
            if(!exludeFiles)
            {
                foreach(var file in Directory.GetFiles(state.Path, filter))
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
                catch(Exception ex) 
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
