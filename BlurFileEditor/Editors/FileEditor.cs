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

    public static FileEditor? CreateFileEditorFor(FileSystemInfo info)
    {
        switch(info.Extension)
        {
            case ".loc":
                return new LocEditor(info);
            default:
                return null;
        }
    }
}
