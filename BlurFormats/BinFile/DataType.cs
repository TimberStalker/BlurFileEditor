using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile;
[DebuggerDisplay("{Name} Items:{Fields.Count}   Unknown:{Unknown}   Read:{ReadType}   Decode:{DecodeType}   Size:{Size}")]
public class DataType
{
    public string Name { get; set; }
    public ushort Unknown { get; private set; }
    public ushort ReadType { get; private set; }
    public ushort DecodeType { get; private set; }
    public ushort Size { get; private set; }
    public List<DataField> Fields { get; }
    public DataType(string name, ushort unknown, ushort readType, ushort decodeType, ushort size)
	{
        Fields = new();
		Name = name;
        Unknown = unknown;
        ReadType = readType;
        DecodeType = decodeType;
        Size = size;
    }

    public override string ToString() => Name;
}
