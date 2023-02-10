using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData;
[DebuggerDisplay("{Name} {DataType?.Name ?? \"None\"} {ReadType} {Offset}")]
public class DataField
{
    public string Name { get; set; }
    public ReadType ReadType { get; set; }
    public int Offset { get; set; }
    public DataType? DataType { get; set; }
    public DataField(string name, int readType, int offset, DataType? structure)
    {
        Name = name;
        ReadType = (ReadType)readType;
        Offset = offset;
        DataType = structure;
    }

    public override string ToString() => Name;
}
