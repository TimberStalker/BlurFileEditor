using BlurFormats.BlurData.Entities;
using BlurFormats.BlurData.Types;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BlurFormats.BlurData;
public sealed class DataType : INotifyPropertyChanged
{
    string name = "", formatString = "";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name 
    { 
        get => name;
        set
        {
            name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
    public DataField? this[string name] => GetField(name);
    public DataType? Base { get; set; }
    public StructureType StructureType { get; private set; }
    public PrimitiveType PrimitiveType { get; private set; }
    public ObservableCollection<DataField> Fields { get; }
    public string FormatString 
    { 
        get => formatString; 
        set
        {
            formatString = value;
            OnPropertyChanged(nameof(FormatString));
        }
    }

    public int Size => StructureType switch
    {
        StructureType.Primitive => 4,
        StructureType.Enum => 4,
        StructureType.Flags => 4,
        StructureType.Struct => (Base?.Size ?? 0) + Fields.Sum(f => f.Size),
        _ => 4
    };

    public DataType(string name, StructureType structureType, PrimitiveType primitiveType)
    {
        Fields = new();
        Name = name;
        StructureType = structureType;
        PrimitiveType = primitiveType;
    }
    public int GetOffset(DataField field)
    {
        int offset = Base?.Size ?? 0;

        if (Fields.Contains(field))
        {
            return offset + Fields.IndexOf(field);
        }

        return -1;
    }
    public DataField? GetField(string name) => Fields.FirstOrDefault(f => f.Name == name);
    public bool IsSubclassOf(DataType type) => this == type ? true : Base?.IsSubclassOf(type) ?? false;
    public override string ToString() => Name;
    public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
