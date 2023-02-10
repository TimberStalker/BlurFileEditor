using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData;
[DebuggerDisplay("{Name} Items:{Fields.Count}   Parent:{Parent?.Name ?? \"None\"}   Read:{ReadType}   Decode:{DecodeType}   Size:{Size}")]
public class DataType
{
    public string Name { get; set; }
    public DataType? Parent { get; set; }
    public ReadType ReadType { get; private set; }
    public DecodeType DecodeType { get; private set; }
    public ushort Size { get; private set; }
    public List<DataField> Fields { get; }
    public DataType(string name, ushort readType, ushort decodeType, ushort size)
    {
        Fields = new();
        Name = name;
        ReadType = (ReadType)readType;
        DecodeType = (DecodeType)decodeType;
        Size = size;
    }

    public override string ToString() => Name;
}
