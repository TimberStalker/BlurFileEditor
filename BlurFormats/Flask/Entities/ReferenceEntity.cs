using BlurFormats.BlurData.Entities;
using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities;
public class ReferenceEntity : IEntity, IReferenceEntity
{
    public SerializationType Type { get; }

    public IEntity Reference { get; private set; }
    public object Value => Reference.Value;
    public IEnumerable<object> Children => Reference.Children;

    public bool DisplaySimple => Reference.DisplaySimple;

    public ReferenceEntity(SerializationType type, IEntity reference)
    {
        Type = type;
        Reference = reference;
    }

    public void SetReference(IEntity reference) => Reference = reference;

    public void Serialize(BinaryWriter writer)
    {
        Reference.Serialize(writer);
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
        if(Reference is StructureEntity)
        {
            writer.WriteStartElement("Reference");
            writer.WriteAttributeString("object_id", Reference.GetHashCode().ToString());
            //Reference.WriteXml(writer);
            writer.WriteEndElement();
        } else
        {
            Reference.WriteXml(writer);
        }
    }

    public IEntity Clone(Dictionary<IEntity, IEntity> mappings)
    {
        if(!mappings.TryGetValue(Reference, out var mapping))
        {
            mappings[Reference] = mapping = Reference.Clone();
        }

        return new ReferenceEntity(Type, mapping);
    }
}
