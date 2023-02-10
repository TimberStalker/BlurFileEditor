using BlurFileEditor.Pages;
using BlurFileEditor.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Editors;
public class LocEditor : FileEditor
{
    object editorContent;
    public LocEditor(FileSystemInfo info) : base(info)
    {
        var editorPage = new LocEditorPage();

        var model = (LocEditorViewModel)editorPage.DataContext;
        model.SetFileSource(info);

        this.editorContent = editorPage;
    }

    public override object EditorContent => editorContent;
}
