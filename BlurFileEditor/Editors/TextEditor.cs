using BlurFileEditor.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Editors;
[FileEditor(".txt")]
public class TextEditor : FileEditor
{
    object editorContent;
    public TextEditor(FileSystemInfo info) : base(info)
    {
        editorContent = new TextEditorPage();
    }

    public override object EditorContent => editorContent;
}
