using System.Runtime.InteropServices;
using BlurFormats.Utils;

namespace BlurFormats.Serialization.Definitions;
[StructLayout(LayoutKind.Sequential)]
public struct DataTypeDefinition
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
    public void Writer(BinaryWriter writer)
    {
        writer.Write(StringOffset);
        writer.Write(FieldCount);
        writer.Write(HasBase);
        writer.Write(StructureType);
        writer.Write(PrimitiveType);
        writer.Write(Size);
    }
    public static DataTypeDefinition Read(BinaryReader reader)
    {
        var stringOffset = reader.ReadInt16();
        var fieldCount = reader.ReadInt16();
        var hasBase = reader.ReadInt16();
        var structureType = reader.ReadInt16();
        var primitiveType = reader.ReadInt16();
        var size = reader.ReadInt16();
        return new DataTypeDefinition(stringOffset, fieldCount, hasBase, structureType, primitiveType, size);
    }
}