using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Read;
[DebuggerDisplay("Type:{BaseType} Specifics:{EntitySpecifics} - {GlobalID}")]
public struct RecordDefinition : IReadable
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
    public void Read(ref Reader reader)
    {
        ID = reader.ReadUInt();
        BaseType = reader.ReadShort();
        Entity = reader.ReadShort();
    }
    public static void Read(BinaryReader reader, ref RecordDefinition record)
    {
        uint id = reader.ReadUInt32();
        short baseType = reader.ReadInt16();
        short entity = reader.ReadInt16();
        record = new RecordDefinition(id, baseType, entity);
    }
}
