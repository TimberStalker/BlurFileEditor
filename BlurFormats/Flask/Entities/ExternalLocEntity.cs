using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities;
public class ExternalLocEntity : IEntity
{
    public PrimitiveType Type { get; }
    public int Offset { get; }
    SerializationType IEntity.Type => Type;
    public object Value => Offset;
    public IEnumerable<object> Children => Array.Empty<IEntity>();


    public bool DisplaySimple => true;

    public ExternalLocEntity(PrimitiveType type, int offset)
    {
        Type = type;
        Offset = offset;
    }

    public void Serialize(BinaryWriter writer)
    {
        Type.SerializeValue(writer, this);
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
        writer.WriteStartElement(Type.Name);
        writer.WriteAttributeString("id", Offset.ToString());
        writer.WriteEndElement();
    }
}
