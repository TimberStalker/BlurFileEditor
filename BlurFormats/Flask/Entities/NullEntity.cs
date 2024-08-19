using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities;
public class NullEntity : IEntity
{
    public SerializationType Type { get; }

    public object Value => -1;

    public IEnumerable<object> Children => Array.Empty<IEntity>();

    public bool DisplaySimple => true;

    public NullEntity(SerializationType type)
    {
        Type = type;
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
        writer.WriteStartElement("Null");
        writer.WriteAttributeString("type", Type.Name);
        writer.WriteEndElement();
    }
}
