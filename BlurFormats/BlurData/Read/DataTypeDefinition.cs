using System.Runtime.InteropServices;
using BlurFormats.Utils;

namespace BlurFormats.BlurData.Read;
public struct DataTypeDefinition : IReadable
{
    public short StringOffset { get; private set; }
    public short FieldCount { get; private set; }
    public short HasBase { get; private set; }
    public short StructureType { get; private set; }
    public short PrimitiveType { get; private set; }
    public short Size { get; private set; }
    public DataTypeDefinition(short stringOffset, short fieldCount, short hasBase, short structureType, short primitiveType, short size)
    {
        StringOffset = stringOffset;
        FieldCount = fieldCount;
        HasBase = hasBase;
        StructureType = structureType;
        PrimitiveType = primitiveType;
        Size = size;
    }
    public void Read(ref Reader reader)
    {
        StringOffset = reader.ReadShort();
        FieldCount = reader.ReadShort();
        HasBase = reader.ReadShort();
        StructureType = reader.ReadShort();
        PrimitiveType = reader.ReadShort();
        Size = reader.ReadShort();
    }
}