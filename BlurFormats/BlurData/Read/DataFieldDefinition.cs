using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Read;
public struct DataFieldDefinition : IReadable
{
    public short NameOffset { get; private set; }
    public short BaseType { get; private set; }
    public short Offset { get; private set; }
    public short FieldType { get; private set; }
    public DataFieldDefinition(short nameOffset, short baseType, short offset, short fieldType)
    {
        NameOffset = nameOffset;
        BaseType = baseType;
        Offset = offset;
        FieldType = fieldType;
    }
    public void Read(ref Reader reader)
    {
        NameOffset = reader.ReadShort();
        BaseType = reader.ReadShort();
        Offset = reader.ReadShort();
        FieldType = reader.ReadShort();
    }
}
