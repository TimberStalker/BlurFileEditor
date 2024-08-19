using BlurFormats.Serialization.Definitions;
using BlurFormats.Serialization.Types;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Flask;
public class FlaskValidator
{
    public bool IsValid(BinaryReader reader, [NotNullWhen(false)]out string? message)
    {
        message = null;

        int format = reader.ReadInt32();
        if (format != 0x464C534B)
        {
            message = WriteLog(reader, sizeof(int), "File is not of the Flask binary serialization format.");
            return false;
        }

        int version = reader.ReadInt32();
        if (version != 2)
        {
            message = WriteLog(reader, sizeof(int), $"Flask version '{version}' is not supported. Must Be version '2'.");
            return false;
        }

        if(!TryGetHeader(reader, "types",    0x555_5555,  out var dataTypesHeader,   out message)) return false;
        if(!TryGetHeader(reader, "parents",  0x1000_0000, out var inheritenceHeader, out message)) return false;
        if(!TryGetHeader(reader, "fields",   0x800_0000,  out var dataFieldsHeader,  out message)) return false;
        if(!TryGetHeader(reader, "names",    0x4000_0000, out var typeNamesHeader,   out message)) return false;
        if(!TryGetHeader(reader, "records",  0x800_0000,  out var recordsHeader,     out message)) return false;
        if(!TryGetHeader(reader, "entities", 0x400_0000,  out var entitiesHeader,    out message)) return false;
        if(!TryGetHeader(reader, "blocks",   0xaaa_aaaa,  out var blocksHeader,      out message)) return false;
        if(!TryGetHeader(reader, "bytes",    0x4000_0000, out var dataHeader,        out message)) return false;

        DataTypeDefinition[] typeDefinitions = new DataTypeDefinition[dataTypesHeader.Length];
        for (int i = 0; i < dataTypesHeader.Length; i++)
        {
            var definition = DataTypeDefinition.Read(reader);

            
            if (definition.StructureType > (int)Structures.Struct)
            {
                message = WriteLog(reader, 12, $"The structure type of {definition.StructureType} at type[{i}] is not a valid structure type");
                return false;
            }
            if(definition.StructureType == (int)Structures.Primitive)
            {
                if(definition.PrimitiveType == (int)Primitives.None)
                {
                    message = WriteLog(reader, 12, $"The primitve type of type[{i}] is None even though type[{i}] is a primitve type");
                    return false;
                }
                if(definition.FieldCount != 0)
                {
                    message = WriteLog(reader, 12, $"The field count of type[{i}] is != 0 even though type[{i}] is a primitve type");
                    return false;
                }
            } else if(definition.PrimitiveType != (int)Primitives.None)
            {
                message = WriteLog(reader, 12, $"The primitve type of type[{i}] is {(Primitives)definition.PrimitiveType} even though type[{i}] is not a primitve type");
                return false;
            }
            if(definition.StructureType != (int)Structures.Struct && definition.HasBase != 0)
            {
                message = WriteLog(reader, 12, $"The {(Structures)definition.StructureType} type of type[{i}] has a base type even though type[{i}] is not a structure type");
                return false;
            }

            typeDefinitions[i] = definition;
        }

        return false;
    }
    public string WriteLog(BinaryReader reader, int offset, string message)
    {
        return $"@{reader .BaseStream.Position - offset:X8} - {message}";
    }
    public bool TryGetHeader(BinaryReader reader, string name, uint maxLength, out Header header, out string message)
    {
        header = Header.Read(reader);
        if ((header.Start & 0b11) != 0)
        {
            message = WriteLog(reader, 8, $"Flask {name} header is incorrectly aligned.");
            return false;
        }
        else if (header.Length > maxLength)
        {
            message = WriteLog(reader, 8, $"Flask cannot exceed 0x{maxLength:X} {name}. Currently is 0x{header.Length:X}");
            return false;
        }
        message = "";
        return true;
    }
}
