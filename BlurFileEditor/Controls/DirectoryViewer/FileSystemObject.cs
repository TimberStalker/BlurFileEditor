using BlurFileEditor.Utils.Interop.Files;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BlurFileEditor.Controls.DirectoryViewer;
public class FileSystemObject : INotifyPropertyChanged, IEnumerable<FileSystemObject>
{
    ObservableCollection<FileSystemObject>? children;
    ImageSource? imageSource;
    bool isExpanded;
    FileSystemInfo fileSystemInfo;
    DriveInfo? drive;
    public ObservableCollection<FileSystemObject>? Children
    {
        get => children;
        private set 
        {
            children = value;
            UpdateProperty(nameof(Children));
        }
    }

    public ImageSource? ImageSource
    {
        get => imageSource;
        private set
        {
            imageSource = value;
            UpdateProperty(nameof(ImageSource));
        }
    }

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            isExpanded = value;
            IsExpandedChanged();
            UpdateProperty(nameof(IsExpanded));
        }
    }
    public FileSystemInfo FileSystemInfo
    {
        get => fileSystemInfo;
        private set
        {
            fileSystemInfo = value;
            UpdateProperty(nameof(FileSystemInfo));
        }
    }

    private DriveInfo? Drive
    {
        get => drive;
        set
        {
            drive = value;
            UpdateProperty(nameof(Drive));
        }
    }

    bool hasBeenChecked;

    public event EventHandler? OnBeforeExpand;
    public event EventHandler? OnAfterExpand;
    public event EventHandler? OnBeforeExplore;
    public event EventHandler? OnAfterExplore;
    public event EventHandler<FileSystemObject>? OnCreatedChild;

    public event EventHandler<FileSystemInfo>? OnFileOpened;

    public FileSystemObject(DriveInfo drive)
        : this(drive.RootDirectory)
    {
    }
    public FileSystemObject(FileSystemInfo info)
    {
        fileSystemInfo = info;
        FileSystemInfo = info;

        if (info is DirectoryInfo)
        {
            Children = new ObservableCollection<FileSystemObject> { new DummyFileSystemObject() };
            ImageSource = FolderManager.GetImageSource(info.FullName, ItemState.Close);
        }
        else if (info is FileInfo)
        {
            ImageSource = FileManager.GetImageSource(info.FullName);
        }
    }
    void IsExpandedChanged()
    {
        if(FileSystemInfo is DirectoryInfo)
        {
            BeforeExpand();
            if (IsExpanded)
            {
                ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Open);
                if (!hasBeenChecked)
                {
                    hasBeenChecked = true;
                    ExploreChildren();
                }
            }
            else
            {
                ImageSource = FolderManager.GetImageSource(FileSystemInfo.FullName, ItemState.Close);
            }
            AfterExpand();
        }
        else
        {
            OnFileOpened?.Invoke(this, FileSystemInfo);
        }
    }

    private void BeforeExpand() => OnBeforeExpand?.Invoke(this, EventArgs.Empty);
    private void AfterExpand() => OnAfterExpand?.Invoke(this, EventArgs.Empty);
    private void BeforeExplore() => OnBeforeExplore?.Invoke(this, EventArgs.Empty);
    private void AfterExplore() => OnAfterExplore?.Invoke(this, EventArgs.Empty);


    private void ExploreDirectories()
    {
        if (Drive?.IsReady == false)
        {
            return;
        }
        if (FileSystemInfo is DirectoryInfo)
        {
            var directories = ((DirectoryInfo)FileSystemInfo).GetDirectories();
            foreach (var directory in directories.OrderBy(d => d.Name))
            {
                if ((directory.Attributes & FileAttributes.System) != FileAttributes.System &&
                    (directory.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    var fileSystemObject = new FileSystemObject(directory);
                    fileSystemObject.OnBeforeExplore += (sender, e) => BeforeExplore();
                    fileSystemObject.OnAfterExplore += (sender, e) => AfterExplore();
                    OnCreatedChild?.Invoke(this, fileSystemObject);
                    Children!.Add(fileSystemObject);
                }
            }
        }
    }

    private void ExploreFiles()
    {
        if (Drive?.IsReady == false)
        {
            return;
        }
        if (FileSystemInfo is DirectoryInfo)
        {
            var files = ((DirectoryInfo)FileSystemInfo).GetFiles();
            foreach (var file in files.OrderBy(d => d.Name))
            {
                if ((file.Attributes & FileAttributes.System) != FileAttributes.System &&
                    (file.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                    var fileObject = new FileSystemObject(file);
                    OnCreatedChild?.Invoke(this, fileObject);
                    Children!.Add(fileObject);
                }
            }
        }
    }
    void ExploreChildren()
    {
        Children?.RemoveAt(0);
        BeforeExplore();
        
        ExploreDirectories();
        ExploreFiles();
        
        AfterExplore();
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void UpdateProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public IEnumerator<FileSystemObject> GetEnumerator() => new List<FileSystemObject> { this }.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
