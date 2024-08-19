using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlurFormats.Serialization.Types;
public class EnumType : SerializationType
{
    public override int Size => 4;
    public bool IsFlags { get; set; }
    public List<string> Values { get; set; } = new List<string>();


    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("Enum");
        writer.WriteAttributeString("flags", IsFlags.ToString());
        foreach (var item in Values)
        {
            writer.WriteStartElement(item);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }
}
