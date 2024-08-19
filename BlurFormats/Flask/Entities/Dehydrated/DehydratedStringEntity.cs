using BlurFormats.Serialization.Types;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace BlurFormats.Serialization.Entities.Dehydrated;
public class DehydratedStringEntity : IEntity, IDehydratedEntity
{
    public PrimitiveType Type { get; }
    public int Offset { get; }
    SerializationType IEntity.Type => Type;
    public object Value => Offset;
    public IEnumerable<object> Children => Array.Empty<IEntity>();

    public int Priority => DehydratedEntities.PrimitivePriority;

    public bool DisplaySimple => false;

    public DehydratedStringEntity(PrimitiveType type, int offset)
    {
        Type = type;
        Offset = offset;
    }

    public IEntity GetReplacement(Dictionary<string, object> pointerData)
    {
        if(pointerData.TryGetValue("Text", out var text))
        {
            var chars = (char[])text;
            return new PrimitiveEntity(Type, chars.GetTerminatedString(Offset));
        }
        throw new NotImplementedException();
    }

    public void Serialize(BinaryWriter writer)
    {
        writer.Write(Offset);
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
        ;
    }
}
