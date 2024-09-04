using BlurFileEditor.Pages;
using BlurFileEditor.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Editors;
[FileEditor(".cpmodel")]
public class ModelEditor : FileEditor
{
    public ModelEditor(FileSystemInfo info) : base(info)
    {
        var editorPage = new ModelEditorPage();

        var model = (ModelEditorViewModel)editorPage.DataContext;

        model.SetInfo(info);

        editorContent = editorPage;
    }
    object editorContent;
    public override object EditorContent => editorContent;
}
