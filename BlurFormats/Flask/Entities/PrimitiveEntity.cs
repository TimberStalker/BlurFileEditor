using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities;
public class PrimitiveEntity : IEntity
{
    public PrimitiveType Type { get; }
    SerializationType IEntity.Type => Type;

    public object Value { get; set; }

    public IEnumerable<object> Children => Array.Empty<IEntity>();

    public bool DisplaySimple => true;

    public PrimitiveEntity(PrimitiveType type, object value)
    {
        Type = type;
        Value = value;
        Debug.Assert(type.InternalType == value.GetType());
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
        writer.WriteAttributeString("value", Value.ToString());
        writer.WriteEndElement();
    }
}
