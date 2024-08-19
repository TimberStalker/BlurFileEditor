using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities.Dehydrated;
[DebuggerDisplay("{Type.Name} ({BlockId}, {Offset})")]
public class DehydratedPointerEntity : IEntity, IDehydratedEntity
{
    public SerializationType Type { get; }
    public virtual short BlockId { get; }
    public short Offset { get; }

    public object Value => (BlockId, Offset);

    public IEnumerable<object> Children => Array.Empty<IEntity>();

    public int Priority => DehydratedEntities.EntityPriority;

    public bool DisplaySimple => true;

    public DehydratedPointerEntity(SerializationType type, short blockId, short offset)
    {
        Type = type;
        BlockId = blockId;
        Offset = offset;
    }

    public IEntity GetReplacement(Dictionary<string, object> pointerData)
    {
        if(pointerData.TryGetValue("Block", out var value))
        {
            if(BlockId == -1) return new NullEntity(Type);
            var blocks = (List<List<IEntity>>)value;
            var block = blocks[BlockId];
            var entity = block[Offset];
            return entity;
        }
        throw new NotImplementedException();
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(BlockId);
        writer.Write(Offset);
    }

    public XmlSchema? GetSchema()
    {
        throw new NotImplementedException();
    }

    public void ReadXml(XmlReader reader)
    {
        throw new NotImplementedException();
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("Pointer");
        writer.WriteAttributeString("type", Type.Name);
        writer.WriteAttributeString("block", BlockId.ToString());
        writer.WriteAttributeString("offset", Offset.ToString());
        writer.WriteEndElement();
    }
}
public class DehydratedBlockResolvingPointerEntity : DehydratedPointerEntity
{
    public override short BlockId => (short)BlockIdResolver();

    public Func<int> BlockIdResolver { get; }

    public DehydratedBlockResolvingPointerEntity(SerializationType type, Func<int> blockIdResolver, short offset) : base(type, -1, offset)
    {
        BlockIdResolver = blockIdResolver;
    }
}
