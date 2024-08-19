using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BlurFormats.Serialization;
public class SerializationRecord : IXmlSerializable
{
    public uint ID { get; set; }
    public IEntity Entity { get; set; }
    public List<IEntity> RecordHeap { get; }
    public IEnumerable<IEntity> EntityAsEumerable => new[] { Entity };
    public SerializationRecord(uint id, IEntity entity, List<IEntity> recordHeap)
    {
        ID = id;
        Entity = entity;
        RecordHeap = recordHeap;
    }
    public SerializationRecord(uint id, IEntity entity) : this(id, entity, new List<IEntity>()) { }

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
        writer.WriteStartElement("Record");
        writer.WriteAttributeString("id", ID.ToString());
        writer.WriteStartElement("Entity");
        Entity.WriteXml(writer);
        writer.WriteEndElement();
        writer.WriteStartElement("Heap");
        foreach (var item in RecordHeap)
        {
            writer.WriteStartElement("Entity");
            writer.WriteAttributeString("object_id", item.GetHashCode().ToString());
            item.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.WriteEndElement();
    }
}
