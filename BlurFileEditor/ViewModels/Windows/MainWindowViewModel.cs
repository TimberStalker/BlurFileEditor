using BlurFileEditor.Editors;
using BlurFileEditor.Utils;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BlurFileEditor.ViewModels.Windows;
public class MainWindowViewModel : ObservableObject
{
    private OpenFolderDialog directoryOpenerDialogue;

    public string EditorDirectory { get; set; }
    public ObservableCollection<FileEditor> OpenEditors { get; }
    public ICommand OpenFileCommand { get; }
    public ICommand SetEditorDirectory { get; }
    public int SelectedTab { get; set; }

	public MainWindowViewModel()
	{
        EditorDirectory = "";
        directoryOpenerDialogue = new OpenFolderDialog() 
        {
            AddToRecent = true,
            Multiselect = false,
        };

        OpenEditors = new ObservableCollection<FileEditor>();
        OpenFileCommand = new Command<FileSystemInfo>(info =>
        {
            if(info is null)
            {
                return;
            }
            var existingEditor = OpenEditors.FirstOrDefault(e => e.Info == info);

            if (existingEditor != null)
            {
                SelectedTab = OpenEditors.IndexOf(existingEditor);
                UpdateProperty(nameof(SelectedTab));
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
            var result = directoryOpenerDialogue.ShowDialog();
            if(result == true)
            {
                    EditorDirectory = directoryOpenerDialogue.FolderName;
                    OpenEditors.Clear();

                    UpdateProperty(nameof(OpenEditors));
                    UpdateProperty(nameof(EditorDirectory));
            }
        });
    }

    public void MoveEditorTo(int originalIndex, int destinationIndex)
    {
        var swappedEditor = OpenEditors[originalIndex];
        OpenEditors.RemoveAt(originalIndex);
        OpenEditors.Insert(destinationIndex, swappedEditor);
        SelectedTab = destinationIndex;
        UpdateProperty(nameof(OpenEditors));
        UpdateProperty(nameof(SelectedTab));
    }
}
