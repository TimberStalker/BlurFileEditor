using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlurFormats.Serialization.Types;
[DebuggerDisplay("{Name} [{Fields.Count}] : {Base?.Name ?? \"None\"} [{FullFields.Count()}]")]
public class StructureType : SerializationType
{
    public int BaseSize { private get; init; }
    public int BaseFieldCount { private get; init; }
    public override int Size
    {
        get 
        { 
            var size = FullFields.Sum(f => f.Size); 
            if(size > 0)
            {
                return size;
            }
            return BaseSize;
        }
    }
    public int FieldCount => Fields.Count > 0 ? Fields.Count : BaseFieldCount;
    public StructureType? Base { get; set; }
    public List<StructureField> Fields { get; set; } = new List<StructureField>();
    public IEnumerable<StructureField> FullFields => Base is null ? Fields : Base.FullFields.Concat(Fields);

    public bool IsExternal => Fields.Count == 0;
    public bool IsSubclassOf(SerializationType type)
    {
        if (this == type) return true;
        if (Base is null) return false;
        return Base.IsSubclassOf(type);
    }
    public bool IsSubclassOf(string? type)
    {
        if (string.IsNullOrEmpty(type)) return false;
        if (Name == type) return true;
        if (Base is null) return false;
        return Base.IsSubclassOf(type);
    }

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement("Structure");
        writer.WriteAttributeString("name", Name);
        if(Base is not null) writer.WriteAttributeString("base", Base.Name);
        foreach (var item in Fields)
        {
            writer.WriteStartElement(item.Type.Name);
            writer.WriteAttributeString("name", item.Name);
            writer.WriteAttributeString("pointer", item.PointerType.ToString());
            writer.WriteAttributeString("isArray", item.IsArray.ToString());
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }
}
public class StructureField
{
    public string Name { get; set; } = "";
    public required StructureType Base { get; set; }
    public required SerializationType Type { get; set; }
    public PointerTypes PointerType { get; set; }
    public bool IsArray { get; set; }
    public int Size
    {
        get
        {
            if(IsArray)
            {
                return 8;
            }
            else
            {
                return PointerType switch
                {
                    PointerTypes.Reference => Type.Size,
                    _ => 4
                };
            }
        }
    }
    public string DisplayType => $"{Type.Name}{PointerType switch { PointerTypes.Pointer => "*" , PointerTypes.External => "^" , _ => "" }}{(IsArray ? "[]": "")}";
}
