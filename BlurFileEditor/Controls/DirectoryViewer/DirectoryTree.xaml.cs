using BlurFileEditor.Utils.Interop.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlurFileEditor.Controls.DirectoryViewer;
/// <summary>
/// Interaction logic for DirectoryTree.xaml
/// </summary>
public partial class DirectoryTree : UserControl, INotifyPropertyChanged
{
    FileSystemObject? rootFileObject;
    internal FileSystemObject? RootFileObject 
    { 
        get => rootFileObject;
        private set
        {
            rootFileObject = value;
            UpdateProperty(nameof(RootFileObject));
        }
    }
    public string? RootDirectory
    {
        get { return (string?)GetValue(RootDirectoryProperty); }
        set { SetValue(RootDirectoryProperty, value); }
    }


    public ICommand OnFileOpened
    {
        get { return (ICommand)GetValue(OnFileOpenedProperty); }
        set { SetValue(OnFileOpenedProperty, value); }
    }

    // Using a DependencyProperty as the backing store for OnFileOpened.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OnFileOpenedProperty =
        DependencyProperty.Register("OnFileOpened", typeof(ICommand), typeof(DirectoryTree), new FrameworkPropertyMetadata(null));


    // Using a DependencyProperty as the backing store for SourcePath.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RootDirectoryProperty =
        DependencyProperty.Register("RootDirectory", typeof(string), typeof(DirectoryTree), new FrameworkPropertyMetadata("", RootDirectoryChanged));

    
    public event PropertyChangedEventHandler? PropertyChanged;

    static void RootDirectoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var directoryTree = (DirectoryTree)d;
        if (e.NewValue is null)
        {
            directoryTree.RootFileObject = null;
        }
        else
        {
            directoryTree.RootFileObject = new FileSystemObject(new DirectoryInfo((string)e.NewValue));
        }
    }

    public void OpenFile(FileSystemInfo info) => OnFileOpened.Execute(info);

    public DirectoryTree()
    {
        InitializeComponent();
    }
    public void UpdateProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
