using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile;
[DebuggerDisplay("{Name} {DataType?.Name ?? \"None\"} {ReadType} {Offset}")]
public class DataField
{
    public string Name { get; set; }
    public int ReadType { get; set; }
    public int Offset { get; set; }
    public DataType? DataType { get; set; }
    public DataField(string name, int readType, int offset, DataType? structure)
    {
        Name = name;
        ReadType = readType;
        Offset = offset;
        DataType = structure;
    }
}
