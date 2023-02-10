using BlurFileEditor.Editors;
using BlurFileEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace BlurFileEditor.ViewModels.Windows;
public class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<FileEditor> OpenEditors { get; }
    public ICommand OpenFileCommand { get; }

	public MainWindowViewModel()
	{
        OpenEditors = new ObservableCollection<FileEditor>
        {
            FileEditor.CreateFileEditorFor(new FileInfo("C:\\Users\\ChrisG\\Desktop\\BlurModified\\gamedata\\xt\\loc\\fandemands.loc"))!
        };
        OpenFileCommand = new Command<FileSystemInfo>(info =>
        {
            if(info is null)
            {
                return;
            }
            var editor = FileEditor.CreateFileEditorFor(info);
            if(editor is not null)
            {
                OpenEditors.Add(editor);
                UpdateProperty(nameof(OpenEditors));
            }
        });
    }
}
