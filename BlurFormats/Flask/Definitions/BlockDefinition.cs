using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Serialization.Definitions;
[DebuggerDisplay("{DataType} {Count} {PointerType}")]
public struct BlockDefinition
{
    public short DataType { get; set; }
    public short Count { get; set; }
    public short PointerType { get; set; }
    public BlockDefinition(short dataType, short count, short pointerType)
    {
        DataType = dataType;
        Count = count;
        PointerType = pointerType;
    }
    public static BlockDefinition Read(BinaryReader reader)
    {
        short dataType = reader.ReadInt16();
        short count = reader.ReadInt16();
        short pointerType = reader.ReadInt16();
        return new BlockDefinition(dataType, count, pointerType);
    }
}
