using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Read;
[DebuggerDisplay("Entity:{Entity} L:{Length} S:{Size} O:{Offset}")]
public struct EntityDefinition : IReadable
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
    public void Read(ref Reader reader)
    {
        Record = reader.ReadInt();
        Length = reader.ReadInt();
        Size = reader.ReadInt();
        NamesLength = reader.ReadInt();
    }
    public static void Read(BinaryReader reader, out EntityDefinition entityDefinition)
    {
        int record = reader.ReadInt32();
        int length = reader.ReadInt32();
        int size = reader.ReadInt32();
        int namesLength = reader.ReadInt32();

        entityDefinition = new EntityDefinition(record, length, size, namesLength);
    }
}
