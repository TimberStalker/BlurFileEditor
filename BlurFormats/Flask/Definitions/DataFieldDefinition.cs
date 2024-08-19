using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Serialization.Definitions;
public struct DataFieldDefinition
{
    public short NameOffset { get; private set; }
    public short BaseType { get; private set; }
    public short Offset { get; private set; }
    public byte FieldType { get; private set; }
    public byte IsArray { get; private set; }
    public DataFieldDefinition(short nameOffset, short baseType, short offset, byte fieldType, byte isArray)
    {
        NameOffset = nameOffset;
        BaseType = baseType;
        Offset = offset;
        FieldType = fieldType;
        IsArray = isArray;
    }
    public void Writer(BinaryWriter writer)
    {
        writer.Write(NameOffset);
        writer.Write(BaseType);
        writer.Write(Offset);
        writer.Write(FieldType);
    }
    public static DataFieldDefinition Read(BinaryReader reader)
    {
        var nameOffset = reader.ReadInt16();
        var baseType = reader.ReadInt16();
        var offset = reader.ReadInt16();
        var fieldType = reader.ReadByte();
        var isArray = reader.ReadByte();
        return new DataFieldDefinition(nameOffset, baseType, offset, fieldType, isArray);
    }
}
