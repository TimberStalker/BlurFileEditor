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
[DebuggerDisplay("{Type.Name} - {Values.Count}")]
public class StructureEntity : IEntity
{
    public StructureType Type { get; }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public List<IEntity> Values { get; }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public IEnumerable<StructureFieldValue> GetPairdValues => Type.FullFields.Zip(Values, (f, e) => new StructureFieldValue(f, e));
    SerializationType IEntity.Type => Type;

    public object Value => GetPairdValues;
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public IEnumerable<object> Children => GetPairdValues;

    public bool DisplaySimple => false;

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    StructureFieldValue[] DebuggerGetPairdVaules => GetPairdValues.ToArray();

    public StructureFieldValue this[int i] => new StructureFieldValue(Type.Fields[i], Values[i]); 

    public StructureEntity(StructureType type)
    {
        Type = type;
        Values = new List<IEntity>();
    }
    public StructureEntity(StructureType type, IEnumerable<IEntity> values)
    {
        Type = type;
        Values = new List<IEntity>(values);
    }
    StructureEntity(StructureType type, List<IEntity> values)
    {
        Type = type;
        Values = values;
    }
    public static StructureEntity CreateWithList(StructureType type, List<IEntity> values)
    {
        return new StructureEntity(type, values);
    }

    public void Serialize(BinaryWriter writer)
    {
        foreach (var item in Values)
        {
            item.Serialize(writer);
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
        writer.WriteStartElement(Type.Name);
        foreach (var item in GetPairdValues)
        {
            item.WriteXml(writer);
        }
        writer.WriteEndElement();
    }
}
[DebuggerDisplay("{Field.DisplayType} {Field.Name} = {Entity.Value}")]
public record StructureFieldValue(StructureField Field, IEntity Entity) : IXmlSerializable
{
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
        writer.WriteStartElement("Field");
        writer.WriteAttributeString("field", Field.Name);
        writer.WriteAttributeString("type", Field.Type.Name);
        Entity.WriteXml(writer);
        writer.WriteEndElement();
    }
}