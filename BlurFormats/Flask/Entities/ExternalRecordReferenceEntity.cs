using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities;
public class ExternalRecordReferenceEntity : IEntity
{
    public SerializationType Type { get; }

    public uint Id { get; }
    public object Value => Id;

    public bool DisplaySimple => false;

    public IEnumerable<object> Children => Array.Empty<IEntity>();
    public ExternalRecordReferenceEntity(SerializationType type, uint id)
    {
        Type = type;
        Id = id;
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
        writer.WriteStartElement("External");
        writer.WriteAttributeString("type", Type.Name);
        writer.WriteAttributeString("id", Id.ToString());
        writer.WriteEndElement();
    }
}
