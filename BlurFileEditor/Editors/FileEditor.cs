using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Editors;
public abstract class FileEditor
{
    public string Name => Info.Name;
    public FileSystemInfo Info { get; }
    public abstract object EditorContent { get; }
    public FileEditor(FileSystemInfo info)
    {
        Info = info;
    }

    static Dictionary<string, Type> validEditors;
    static FileEditor()
    {
        validEditors = FileEditorAttribute.GetFileEditors().ToDictionary(t => t.Item1.Extension, t => t.Item2);
    }
    public static FileEditor? CreateFileEditorFor(FileSystemInfo info)
    {
        if (!validEditors.TryGetValue(info.Extension, out var editorType)) return null;

        return Activator.CreateInstance(editorType, info) as FileEditor;
    }
}
