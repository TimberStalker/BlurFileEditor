using BlurFormats.BlurData.Entities;
using BlurFormats.BlurData.Entities.Pointers;
using BlurFormats.BlurData.Types;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData;
[DebuggerDisplay("{Name} {DataType?.Name ?? \"None\"} : {FieldType}")]
public sealed class DataField
{
    public string Name { get; set; }
    public FieldType FieldType { get; set; }
    public DataType? DataType { get; set; }
    public int Size => FieldType switch 
    {
        FieldType.Primitive => 4,
        FieldType.Enum => 4,
        FieldType.FlagsEnum => 4,
        FieldType.Struct => DataType?.Size ?? 4,
        FieldType.Pointer => 4,
        FieldType.ExternalPointer => 4,
        FieldType.EnumValue => 4,
        FieldType.PrimitiveArray => 8,
        FieldType.EnumArray => 8,
        FieldType.StructArray => 8,
        FieldType.PointerArray => 8,
        FieldType.ExternalArray => 8,
        _ => 4,
    };
    public DataField(string name, FieldType fieldType, DataType? structure)
    {
        Name = name;
        FieldType = fieldType;
        DataType = structure;
    }
    public override string ToString() => Name;
}
