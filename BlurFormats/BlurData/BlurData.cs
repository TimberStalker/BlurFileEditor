using BlurFormats.BlurData.Entities;
using BlurFormats.BlurData.Entities.Pointers;
using BlurFormats.BlurData.Read;
using BlurFormats.BlurData.Types;
using BlurFormats.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BlurFormats.BlurData;
public sealed class BlurData : IXmlSerializable
{
    public ObservableCollection<DataType> DataTypes { get; } = new ObservableCollection<DataType>();
    public ObservableCollection<BlurRecord> Records { get; } = new ObservableCollection<BlurRecord>();
    public BlurData()
    {

    }
    public BlurData(IEnumerable<DataType> dataTypes, IEnumerable<BlurRecord> records)
    {
        foreach (var item in dataTypes) DataTypes.Add(item);
        foreach (var item in records) Records.Add(item);
    }
    public void PrintTypes(TextWriter writeTo)
    {
        foreach (var type in DataTypes)
        {
            if (type.StructureType is StructureType.Enum or StructureType.Flags)
            {
                if ((int)type.StructureType == 2) writeTo.WriteLine("[Flags]");
                writeTo.WriteLine($"enum {type.Name}");
                writeTo.WriteLine("{");
                foreach (var field in type.Fields)
                {
                    writeTo.WriteLine($"\t{field.Name},");
                }
                writeTo.WriteLine("}");
            }
            else
            {
                if (type.Fields.Count == 0) writeTo.Write("external ");
                writeTo.Write($"struct {type.Name}");
                if (type.Base is not null)
                {
                    writeTo.Write($" : {type.Base.Name}");
                }
                writeTo.WriteLine("\n{");
                foreach (var field in type.Fields)
                {
                    if ((int)field.FieldType < 256)
                    {
                        writeTo.WriteLine(field.FieldType switch
                        {
                            FieldType.ExternalPointer => $"\textern {field.DataType!.Name}* {field.Name}; // {field.FieldType}",
                            FieldType.Pointer => $"\t{field.DataType!.Name}* {field.Name}; // {field.FieldType}",
                            _ => $"\t{field.DataType!.Name} {field.Name}; // {field.FieldType}"
                        });
                    }
                    else
                    {
                        writeTo.WriteLine(field.FieldType switch
                        {
                            FieldType.ExternalArray => $"\textern {field.DataType!.Name}*[] {field.Name}; // {field.FieldType}",
                            FieldType.PointerArray => $"\t{field.DataType!.Name}*[] {field.Name}; // {field.FieldType}",
                            _ => $"\t{field.DataType!.Name}[] {field.Name}; // {field.FieldType}"
                        });
                    }
                }
                writeTo.WriteLine("}");
            }
        }
    }
    public unsafe static BlurData Deserialize(Stream stream)
    {
        var deserializer = new BlurDataDeserializer(stream);

        deserializer.DeserializeDataTypes();
        deserializer.DeserializeRecords();

        return deserializer.ToBlurData();
    }
    
    public unsafe static void Serialize(BlurData blurData, Stream stream)
    {
        throw null;
    }

    public unsafe static Stream Serialize(BlurData blurData)
    {
        var result = new MemoryStream();
        Serialize(blurData, result);
        return result;
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
        writer.WriteStartElement("BlurData");

        writer.WriteStartElement("Types");
        foreach (var dataType in DataTypes)
        {
            writer.WriteStartElement("Type");
            writer.WriteAttributeString("Name", dataType.Name);
            writer.WriteAttributeString("StructType", dataType.StructureType.ToString());
            writer.WriteAttributeString("PrimitiveType", dataType.PrimitiveType.ToString());
            if(dataType.Base is not null)
            {
                writer.WriteAttributeString("Base", dataType.Base.Name);
            }
            
            foreach (var field in dataType.Fields)
            {
                writer.WriteStartElement("Field");
                writer.WriteAttributeString("Name", field.Name);
                writer.WriteAttributeString("FieldType", field.FieldType.ToString());
                if(field.DataType is not null)
                {
                    writer.WriteAttributeString("Type", field.DataType.Name);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Records");
        foreach (var record in Records)
        {
            writer.WriteStartElement("Record");
            writer.WriteAttributeString("BaseType", record.BaseType.Name);
            writer.WriteAttributeString("ID", record.ID.ToString());
            List<IEntity> entities = new List<IEntity> { record.Entity };
            for(int i = 0; i < entities.Count; i++)
            {
                WriteEntity(writer, entities[i], entities);
            }
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    private static void WriteEntity(XmlWriter writer, IEntity entity, List<IEntity> entities)
    {
        //switch (entity)
        //{
        //    case ObjectEntity o:
        //        writer.WriteStartElement("Object");
        //        writer.WriteAttributeString("Type", o.DataType.Name);
        //        foreach(var field in o.Fields)
        //        {
        //            switch (field.Field.FieldType)
        //            {
        //                case FieldType.Primitive:
        //                case FieldType.Enum:
        //                case FieldType.FlagsEnum:
        //                case FieldType.EnumValue:
        //                    WriteEntity(writer, field.Value, entities);
        //                    break;
        //                case FieldType.Struct:
        //                    break;
        //                case FieldType.Pointer:
        //                    break;
        //                case FieldType.ExternalPointer:
        //                    break;
        //                case FieldType.PrimitiveArray:
        //                    break;
        //                case FieldType.EnumArray:
        //                    break;
        //                case FieldType.StructArray:
        //                    break;
        //                case FieldType.PointerArray:
        //                    break;
        //                case FieldType.ExternalArray:
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        writer.WriteEndElement();
        //        break;
        //    case PrimitiveEntity p:
        //        writer.WriteStartElement("Primitive");
        //        writer.WriteAttributeString("Type", p.DataType.Name);
        //        writer.WriteValue(p.Value);
        //        writer.WriteEndElement();
        //        break;
        //    case FlagsEnumEntity f:
        //        writer.WriteStartElement("Flags");
        //        writer.WriteAttributeString("Type", f.DataType.Name);
        //        writer.WriteAttributeString("Value", Convert.ToString(f.Value, 2));
        //        writer.WriteValue(string.Join('|', f.DataType.Fields.Where((v, i) => (f.Value >> i) % 2 == 1).Select(f => f.Name)));
        //        writer.WriteEndElement();
        //        break;
        //    case EnumEntity e:
        //        writer.WriteStartElement("Enum");
        //        writer.WriteAttributeString("Type", e.DataType.Name);
        //        writer.WriteValue(e.DataType.Fields[e.Value].Name);
        //        writer.WriteEndElement();
        //        break;
        //};
    }
}
