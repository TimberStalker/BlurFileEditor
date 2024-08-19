using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Serialization.Definitions;
[DebuggerDisplay("Type:{BaseType} Specifics:{EntitySpecifics} - {GlobalID}")]
public struct RecordDefinition
{
    public uint ID { get; private set; }
    public short BaseType { get; private set; }
    public short Entity { get; private set; }
    public RecordDefinition(uint iD, short baseType, short entity)
    {
        ID = iD;
        BaseType = baseType;
        Entity = entity;
    }
    public static RecordDefinition Read(BinaryReader reader)
    {
        uint id = reader.ReadUInt32();
        short baseType = reader.ReadInt16();
        short entity = reader.ReadInt16();
        return new RecordDefinition(id, baseType, entity);
    }
}
