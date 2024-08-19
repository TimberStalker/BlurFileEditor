using BlurFormats.Serialization.Types;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities;
public class EnumEntity : IEntity
{
    public EnumType Type { get; }
    public int Value { get; set; }
    public bool IsFlags => Type.IsFlags;
    SerializationType IEntity.Type => Type;
    object IEntity.Value => Value;
    public IEnumerable<object> Children => Array.Empty<IEntity>();

    public bool DisplaySimple => true;

    public EnumEntity(EnumType type, int value)
    {
        Type = type;
        Value = value;
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(Value);
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
        writer.WriteStartElement("Enum");
        writer.WriteAttributeString("enum", Type.Name);
        if (IsFlags)
        {
            
            writer.WriteAttributeString("names", string.Join(',', GetBitIndecies(Value).Select(i => Type.Values[i]) ) );
            writer.WriteAttributeString("bits", Convert.ToString(Value, 2).PadLeft(Type.Values.Count, '0'));
        }
        else
        {
            writer.WriteAttributeString("name", Type.Values[Value]);
            writer.WriteAttributeString("value", Value.ToString());
        }
        writer.WriteEndElement();
    }

    static IEnumerable<int> GetBitIndecies(int value)
    {
        int i = 0;
        while(value > 0)
        {
            if (value % 2 == 1) yield return i;
            value /= 2;
            i++;
        }
    }
}
