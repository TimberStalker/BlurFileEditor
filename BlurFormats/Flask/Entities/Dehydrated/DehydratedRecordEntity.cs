using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities.Dehydrated;
public class DehydratedRecordEntity : IEntity, IDehydratedEntity
{
    public SerializationType Type { get; }
    public int Offset { get; }
    public object Value => Offset;
    public IEnumerable<object> Children => Array.Empty<IEntity>();

    public int Priority => DehydratedEntities.RecordPriority;

    public bool DisplaySimple => false;

    public DehydratedRecordEntity(SerializationType type, int offset)
    {
        Type = type;
        Offset = offset;
    }

    public IEntity GetReplacement(Dictionary<string, object> pointerData)
    {
        if (pointerData.TryGetValue("Records", out var value))
        {
            if (Offset == -1) return new NullEntity(Type);
            var records = (List<SerializationRecord>)value;
            return new RecordReferenceEntity(Type, records[Offset]);
        }
        throw new NotImplementedException();
    }

    public void Serialize(BinaryWriter writer)
    {
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
        writer.WriteStartElement("External");
        writer.WriteAttributeString("type", Type.Name);
        writer.WriteAttributeString("offset", Offset.ToString());
        writer.WriteEndElement();
    }
}
