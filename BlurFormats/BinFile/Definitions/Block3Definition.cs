using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Definitions;
[DebuggerDisplay("{DataType} {Count} {Unknown2}")]
public struct Block3Definition : IReadable
{
    public ushort DataType { get; set; }
    public ushort Count { get; set; }
    public ushort Unknown2 { get; set; }

    public void Read(ref Reader reader)
    {
        DataType = reader.ReadUShort();
        Count = reader.ReadUShort();
        Unknown2 = reader.ReadUShort();
    }
}
