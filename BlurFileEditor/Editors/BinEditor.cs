﻿using BlurFileEditor.Pages;
using BlurFileEditor.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BlurFileEditor.Editors;
[FileEditor(".bin")]
internal class BinEditor : FileEditor
{
    object editorContent;
    public BinEditor(FileSystemInfo info) : base(info)
    {
        var editorPage = new BinEditorPage();

        var model = (BinEditorViewModel)editorPage.DataContext;
        try
        {
            model.SetPage(editorPage);
            model.SetFileSource(info);

        } catch(Exception ex)
        {
            MessageBox.Show(ex.Message);
        }

        this.editorContent = editorPage;
    }

    public override object EditorContent => editorContent;
}
