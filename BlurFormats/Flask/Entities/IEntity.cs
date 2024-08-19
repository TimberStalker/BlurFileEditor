using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BlurFormats.Serialization.Entities;
public interface IEntity : IXmlSerializable
{
    SerializationType Type { get; }
    object Value { get; }
    bool DisplaySimple { get; }
    void Serialize(BinaryWriter writer);
    IEnumerable<object> Children { get; }
    //IEntity Clone(Dictionary<IEntity, IEntity> mappings);
}
