using BlurFileEditor.Pages;
using BlurFileEditor.Utils;
using BlurFileEditor.Utils.FIlter;
using BlurFileEditor.Windows;
using BlurFormats.BlurData;
using BlurFormats.Serialization;
using BlurFormats.Serialization.Types;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;

namespace BlurFileEditor.ViewModels.Pages;
public class BinEditorViewModel : ObservableObject
{
    static int PageItemCount = 20;
    FileSystemInfo? info;
    string binTypesText = "";
    int currentPage;

    public List<SerializationType> Types { get; set; } = new();
    //public List<SerializationRecord> Records { get; set; } = new();
    public FilteredEntityList Entities { get; } = new FilteredEntityList();
    public IEnumerable<SerializationType> DataTypes => Types;
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
        InspectRecordCommand = new Command<SerializationRecord>((record) =>
        {
            if (record is null) return;
            new RecordInspector(record).Show();
        });
    }
    public void SetFileSource(FileSystemInfo info)
    {
        this.info = info;

        (Types, Entities.Records) = FlaskSerializer.Deserialize(info.FullName);
        UpdateProperty(nameof(DataTypes));
    }
    public class FilteredEntityList : ObservableObject, IEnumerable<SerializationRecord>
    {
        private IEntityFilter? entityFilter;
        private List<SerializationRecord>? records;

        public List<SerializationRecord>? Records { get => records; set => SetProperty(ref records, value); }
        public IEntityFilter? EntityFilter 
        { 
            get => entityFilter; 
            set => SetProperty(ref entityFilter, value); 
        }

        public IEnumerator<SerializationRecord> GetEnumerator()
        {
            if(Records is null) return Enumerable.Empty<SerializationRecord>().GetEnumerator();
            if (EntityFilter is null) return Records.GetEnumerator();
            return Records.Where(r => EntityFilter.Matches(r.Entity)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
