using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Serialization.Definitions;
[DebuggerDisplay("Entity:{Entity} L:{Length} S:{Size} O:{Offset}")]
public struct EntityDefinition
{
    public int Record { get; private set; }
    public int Length { get; private set; }
    public int Size { get; private set; }
    public int NamesLength { get; private set; }
    public EntityDefinition(int record, int length, int size, int namesLength)
    {
        Record = record;
        Length = length;
        Size = size;
        NamesLength = namesLength;
    }
    public static EntityDefinition Read(BinaryReader reader)
    {
        int record = reader.ReadInt32();
        int length = reader.ReadInt32();
        int size = reader.ReadInt32();
        int namesLength = reader.ReadInt32();

        return new EntityDefinition(record, length, size, namesLength);
    }
}
