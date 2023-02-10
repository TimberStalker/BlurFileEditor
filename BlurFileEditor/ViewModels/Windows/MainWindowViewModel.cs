using BlurFileEditor.Editors;
using BlurFileEditor.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BlurFileEditor.ViewModels.Windows;
public class MainWindowViewModel : ViewModelBase
{
    private CommonOpenFileDialog direcotryOpenerDialogue;

    public string EditorDirectory { get; set; }
    public ObservableCollection<FileEditor> OpenEditors { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand SetEditorDirectory { get; }
    public int SelectedTab { get; set; }

	public MainWindowViewModel()
	{
        EditorDirectory = "";
        direcotryOpenerDialogue = new CommonOpenFileDialog() 
        {
            IsFolderPicker = true
        };

        OpenEditors = new ObservableCollection<FileEditor>();
        OpenFileCommand = new Command<FileSystemInfo>(info =>
        {
            if(info is null)
            {
                return;
            }
            var editor = FileEditor.CreateFileEditorFor(info);
            if(editor is null)
            {
                MessageBox.Show($"Could not find a valid editor for '{info.Name}'");
                return;
            }
            OpenEditors.Add(editor);
            SelectedTab = OpenEditors.Count - 1;
            UpdateProperty(nameof(OpenEditors));
            UpdateProperty(nameof(SelectedTab));
        });
        SetEditorDirectory = new Command(() =>
        {
            var result = direcotryOpenerDialogue.ShowDialog();
            switch (result)
            {
                case CommonFileDialogResult.Ok:
                    EditorDirectory = direcotryOpenerDialogue.FileName;
                    OpenEditors.Clear();

                    UpdateProperty(nameof(OpenEditors));
                    UpdateProperty(nameof(EditorDirectory));

                    break;
                case CommonFileDialogResult.None:
                case CommonFileDialogResult.Cancel:
                default:
                    break;
            }
        });
    }
}
