using BlurFileEditor.Pages;
using BlurFileEditor.Utils;
using BlurFileEditor.Windows;
using BlurFormats.BlurData;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;

namespace BlurFileEditor.ViewModels.Pages;
public class BinEditorViewModel : ViewModelBase
{
    static int PageItemCount = 20;
    FileSystemInfo? info;
    string binTypesText = "";
    int currentPage;

    public BlurData Bin { get; set; }
    public IEnumerable<BlurRecord> TreeRecords => Bin.Records.Skip(CurrentPage * PageItemCount).Take(PageItemCount);
    public IEnumerable<DataType> DataTypes => Bin.DataTypes.OrderBy(d => d.Name).OrderBy(d => d.StructureType);
    public int CurrentPage 
    { 
        get => currentPage; 
        set
        {
            currentPage = value;
            UpdateProperty(nameof(CurrentPage));
            UpdateProperty(nameof(VisiblePage));
            UpdateProperty(nameof(TreeRecords));
            ((Command)MaxDecrementPageCommand).InvokeCanExecuteChanged();
            ((Command)DecrementPageCommand).InvokeCanExecuteChanged();
            ((Command)IncrementPageCommand).InvokeCanExecuteChanged();
            ((Command)MaxIncrementPageCommand).InvokeCanExecuteChanged();
        } 
    }
    public int VisiblePage => CurrentPage + 1;
    public int TotalPages { get; set; }

    public ICommand MaxDecrementPageCommand { get; }
    public ICommand DecrementPageCommand { get; }
    public ICommand IncrementPageCommand { get; }
    public ICommand MaxIncrementPageCommand { get; }
    public ICommand InspectRecordCommand { get; }
    public string BinTypesText
    {
        get => binTypesText;
        set {
            binTypesText = value;
            UpdateProperty(nameof(BinTypesText));
        }
    }
    public BinEditorViewModel()
    {
        Bin = new BlurData();
        MaxDecrementPageCommand = new Command(() =>
        {
            CurrentPage = 0;
        },
        () =>
        {
            return CurrentPage > 0;
        });
        DecrementPageCommand = new Command(() =>
        {
            CurrentPage--;
        }, 
        () =>
        {
            return CurrentPage > 0;
        });
        IncrementPageCommand = new Command(() =>
        {
            CurrentPage++;
        },
        () =>
        {
            return CurrentPage < TotalPages - 1;
        });
        MaxIncrementPageCommand = new Command(() =>
        {
            CurrentPage = TotalPages-1;
        },
        () =>
        {
            return CurrentPage < TotalPages - 1;
        });
        InspectRecordCommand = new Command<BlurRecord>((record) =>
        {
            if (record is null) return;
            new RecordInspector(record).Show();
        });
    }
    public void SetFileSource(FileSystemInfo info)
    {
        this.info = info;
        using var stream = new FileStream(info.FullName, FileMode.Open, FileAccess.Read);

        var block = BlurData.Deserialize(stream);

        using var textStream = new StringWriter();
        block.PrintTypes(textStream);

        Bin = block;
        BinTypesText = textStream.ToString();
        UpdateProperty(nameof(Bin));
        UpdateProperty(nameof(DataTypes));
        UpdateProperty(nameof(TreeRecords));
        UpdateProperty(nameof(TotalPages));
        
        CurrentPage = 0;
    }
    public void SetPage(BinEditorPage editorPage)
    {
        page = editorPage;
    }
}
