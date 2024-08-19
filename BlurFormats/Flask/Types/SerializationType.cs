using BlurFormats.Serialization.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BlurFormats.Serialization.Types;
public abstract class SerializationType : IXmlSerializable
{
    public string Name { get; set; } = "";
    public abstract int Size { get; }
    public static SerializationType Factory(DataTypeDefinition typeDefinition, string name)
    {
        switch((Structures)typeDefinition.StructureType)
        {
            case Structures.Primitive: 
                return new PrimitiveType() { Name= name, Primitive = (PrimitiveType.Primitives)typeDefinition.PrimitiveType };
            case Structures.Enum: 
                return new EnumType() { Name = name, Values = [] };
            case Structures.Flags: 
                return new EnumType() { Name = name, Values = [], IsFlags= true };
            case Structures.Struct:
                return new StructureType() { Name = name, Fields = [], BaseSize = typeDefinition.Size, BaseFieldCount = typeDefinition.FieldCount};
        }
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

    public abstract void WriteXml(XmlWriter writer);
}
