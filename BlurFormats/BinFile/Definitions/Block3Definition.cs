using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Definitions;
[DebuggerDisplay("{DataType} {Count} {PointerType}")]
public struct Block3Definition : IReadable
{
    public ushort DataType { get; set; }
    public ushort Count { get; set; }
    public ushort PointerType { get; set; }

    public void Read(ref Reader reader)
    {
        DataType = reader.ReadUShort();
        Count = reader.ReadUShort();
        PointerType = reader.ReadUShort();
    }
}
