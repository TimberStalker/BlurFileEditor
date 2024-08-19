using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities;
public class RecordReferenceEntity : IEntity
{
    public SerializationType Type { get; }

    public SerializationRecord Record { get; }

    public object Value => Record;

    public bool DisplaySimple => true;

    public IEnumerable<object> Children => Record.Entity?.Children ?? Array.Empty<IEntity>();

    public RecordReferenceEntity(SerializationType type, SerializationRecord record)
    {
        Type = type;
        Record = record;
    }
    public void Serialize(BinaryWriter writer)
    {
        throw new NotImplementedException();
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
        writer.WriteStartElement("Record");
        writer.WriteAttributeString("id", Record.ID.ToString());
        writer.WriteEndElement();
        //throw new NotImplementedException();
    }
}
