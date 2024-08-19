using BlurFormats.Serialization.Entities.Dehydrated;
using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BlurFormats.Serialization.Entities;
[DebuggerDisplay("{Type.Name}[{Entities.Count}]")]
public class ArrayEntity : IEntity, IXmlSerializable
{
    public SerializationType Type { get; }

    public List<ReferenceEntity> Entities { get; }

    public object Value => Entities;

    public IEnumerable<object> Children => Entities.Select((e, i) => new ArrayEntityItem(i, e));

    public bool DisplaySimple => true;

    public ArrayEntity(SerializationType type)
    {
        Type = type;
        Entities = new();
    }

    public void AddEntity(IEntity entity)
    {
        var reference = new ReferenceEntity(Type, entity);
        Entities.Add(reference);
    }
    public void AddReference(ReferenceEntity entity)
    {
        Entities.Add(entity);
    }

    public void Serialize(BinaryWriter writer)
    {
        if(Entities.Count > 0)
        {
            var startPointer = (DehydratedPointerEntity)Entities[0].Reference;
            writer.Write(startPointer.BlockId);
            writer.Write(startPointer.Offset);
            writer.Write(Entities.Count);
        }
        else
        {
            writer.Write((short)-1);
            writer.Write((short)-1);
            writer.Write(-1);
        }
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
        writer.WriteStartElement("Array");
        writer.WriteAttributeString("type", Type.Name);
        foreach (var item in Entities)
        {
            item.WriteXml(writer);
        }
        writer.WriteEndElement();
    }
}
public record ArrayEntityItem(int Index, ReferenceEntity Reference);