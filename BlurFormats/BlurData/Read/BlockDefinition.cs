using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Read;
[DebuggerDisplay("{DataType} {Count} {PointerType}")]
public struct BlockDefinition : IReadable
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
    public void Read(ref Reader reader)
    {
        DataType = reader.ReadShort();
        Count = reader.ReadShort();
        PointerType = reader.ReadShort();
    }

    public static void Read(BinaryReader reader, out BlockDefinition blockDefinition)
    {
        short dataType = reader.ReadInt16();
        short count = reader.ReadInt16();
        short pointerType = reader.ReadInt16();
        blockDefinition = new BlockDefinition(dataType, count, pointerType);
    }
}
