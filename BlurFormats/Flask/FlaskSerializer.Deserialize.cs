using BlurFormats.Serialization.Definitions;
using BlurFormats.Serialization.Entities;
using BlurFormats.Serialization.Entities.Dehydrated;
using BlurFormats.Serialization.Types;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlurFormats.Serialization;

public partial class FlaskSerializer
{
    public static (List<SerializationType>, List<SerializationRecord>) Deserialize(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        return Deserialize(stream);
    }
    public static (List<SerializationType>, List<SerializationRecord>) Deserialize(Stream stream)
    {
        var serialization = new BlurSerialization();
        var reader = new EndiannessAwareBinaryReader(stream);

        int format = reader.ReadInt32();
        if (format != 0x464C534B)
        {
            if(format == 0x4B534C46)
            {
                reader.Endianness = Endianness.Big;
            }
            throw new Exception("Provided bytes are not of the KSLF(bin) format.");
        }
        int magicNumber = reader.ReadInt32();
        if (magicNumber != 2) throw new Exception($"Magic number is not correct. Should be '2' but instead is '{magicNumber}'.");

        serialization.DataTypesHeader = Header.Read(reader);
        serialization.InheritenceHeader = Header.Read(reader);
        serialization.DataFieldsHeader = Header.Read(reader);
        serialization.TypeNamesHeader = Header.Read(reader);
        serialization.RecordsHeader = Header.Read(reader);
        serialization.EntitiesHeader = Header.Read(reader);
        serialization.BlocksHeader = Header.Read(reader);
        serialization.DataHeader = Header.Read(reader);

        ReadDataTypes(reader, serialization);
        ReadRecords(reader, serialization);

        if (reader.BaseStream.Position % 4 != 0)
        {
            reader.BaseStream.Seek(4 - reader.BaseStream.Position % 4, SeekOrigin.Current);
        }

        var types = DeserializeTypes(serialization);

        var records = ReadAndDeserializeData(reader, serialization, types);

        return (types, records);
    }
    static List<SerializationType> DeserializeTypes(BlurSerialization serialization)
    {
        var types = new List<SerializationType>();

        var inheritenceEnumerator = serialization.InheritenceDefinitions.GetEnumerator();
        var fieldsEnumerator = serialization.FieldDefinitions.GetEnumerator();
        
        foreach (var typeDefinition in serialization.TypeDefinitions)
        {
            var name = serialization.TypeNames.GetTerminatedString(typeDefinition.StringOffset);

            var type = SerializationType.Factory(typeDefinition, name);

            types.Add(type);
        }

        for (int i = 0; i < types.Count; i++)
        {
            var definition = serialization.TypeDefinitions[i];
            var type = types[i];
            switch (type)
            {
                case StructureType s:
                    if (definition.HasBase == 1)
                    {
                        inheritenceEnumerator.MoveNext();
                        s.Base = (StructureType)types[inheritenceEnumerator.Current];
                    }
                    else if (definition.HasBase > 1) throw new NotImplementedException();

                    for (int j = 0; j < definition.FieldCount; j++)
                    {
                        fieldsEnumerator.MoveNext();
                        var fieldDefinition = fieldsEnumerator.Current;

                        var name = serialization.TypeNames.GetTerminatedString(fieldDefinition.NameOffset);
                        var fieldType = (Fields)fieldDefinition.FieldType;
                        bool isArray = fieldDefinition.IsArray > 0;
                        var pointerType = fieldType switch
                        {
                            Fields.Pointer => PointerTypes.Pointer,
                            Fields.ExternalPointer => PointerTypes.External,
                            _ => PointerTypes.Reference
                        };
                        
                        var field = new StructureField { Base = s, Type = types[fieldDefinition.BaseType], Name = name, IsArray = isArray, PointerType = pointerType };

                        s.Fields.Add(field);
                    }
                    break;
                case EnumType e:
                    for (int j = 0; j < definition.FieldCount; j++)
                    {
                        fieldsEnumerator.MoveNext();
                        var fieldDefinition = fieldsEnumerator.Current;

                        var name = serialization.TypeNames.GetTerminatedString(fieldDefinition.NameOffset);

                        e.Values.Add(name);
                    }
                    break;
                case PrimitiveType p:
                    break;
                default:
                    break;
            }
        }

        return types;
    }
    static List<SerializationRecord> ReadAndDeserializeData(BinaryReader reader, BlurSerialization serialization, List<SerializationType> types)
    {
        List<SerializationRecord> records = new List<SerializationRecord>();
        var dehydratedEntities = new PriorityQueue<(ReferenceEntity, IDehydratedEntity), int>();
        int currentBlock = 0;
        for (int i = 0; i < serialization.RecordDefinitions.Count; i++)
        {
            var record = serialization.RecordDefinitions[i];
            if (record.Entity == -1)
            {
                var type = types[record.BaseType];
                records.Add(new SerializationRecord(record.ID, new ExternalRecordReferenceEntity(type, record.ID)));
                continue;
            }
            var entitySegment = serialization.EntityDefinitions[record.Entity];

            List<List<IEntity>> entities = new List<List<IEntity>> { };
            Dictionary<(int block, int offset), ArrayEntity> arrays = new Dictionary<(int block, int offset), ArrayEntity>();
            for (int j = 0; j < entitySegment.Length; j++)
            {
                var block = serialization.BlockDefinitions[currentBlock];
                var type = types[block.DataType];
                List<IEntity> blockEntities = new List<IEntity>();
                for (int k = 0; k < block.Count; k++)
                {
                    IEntity entity = block.PointerType switch
                    {
                        0 => ReadAndDeserializeType(reader, type, arrays, dehydratedEntities),
                        1 => new DehydratedPointerEntity(type, reader.ReadInt16(), reader.ReadInt16()),
                        2 => new DehydratedRecordEntity(type, reader.ReadInt32()),
                        _ => throw new NotSupportedException(),
                    };
                    blockEntities.Add(entity);
                }

                currentBlock++;
                entities.Add(blockEntities);
            }

            var text = new BlurEncoding().GetChars(reader.ReadBytes(entitySegment.NamesLength));
            var pointerData = new Dictionary<string, object>()
            {
                {"Text", text},
                {"Block", entities}
            };

            while (dehydratedEntities.Count > 0 && dehydratedEntities.Peek().Item2.Priority < DehydratedEntities.EntityPriority)
            {
                var (reference, dehydrated) = dehydratedEntities.Dequeue();
                var replacement = dehydrated.GetReplacement(pointerData);
                reference.SetReference(replacement);
                if (replacement is IDehydratedEntity d)
                {
                    dehydratedEntities.Enqueue((reference, d), d.Priority);
                }
            }
            while (dehydratedEntities.Count > 0 && dehydratedEntities.Peek().Item2.Priority < DehydratedEntities.RecordPriority)
            {
                var (reference, dehydrated) = dehydratedEntities.Dequeue();
                var replacement = dehydrated.GetReplacement(pointerData);
                reference.SetReference(replacement);
                if (replacement is IDehydratedEntity d)
                {
                    dehydratedEntities.Enqueue((reference, d), d.Priority);
                }
            }
            List<IEntity> heap = new List<IEntity>();
            foreach (var block in entities)
            {
                foreach (var entity in block)
                {
                    if(entity is not IDehydratedEntity)
                    {
                        heap.Add(entity);
                    }
                }
            }
            foreach(var (key, array) in arrays)
            {
                heap.Add(array);
            }
            heap.Remove(entities[0][0]);
            records.Add(new SerializationRecord(record.ID, entities[0][0], heap));
        }

        var recordPointerData = new Dictionary<string, object>()
        {
            {"Records", records }
        };
        while (dehydratedEntities.Count > 0)
        {
            var (reference, dehydrated) = dehydratedEntities.Dequeue();
            var replacement = dehydrated.GetReplacement(recordPointerData);
            reference.SetReference(replacement);
            if (replacement is IDehydratedEntity d)
            {
                dehydratedEntities.Enqueue((reference, d), d.Priority);
            }
        }
        return records;
    }
    static IEntity ReadAndDeserializeType(BinaryReader reader, SerializationType type, Dictionary<(int block, int offset), ArrayEntity> arrays, PriorityQueue<(ReferenceEntity, IDehydratedEntity), int> dehydratedEntities)
    {
        switch (type)
        {
            case StructureType s:
                List<IEntity> entities = new List<IEntity>();
                foreach (var field in s.FullFields)
                {
                    var entity = ReadAndDeserializeField(reader, field, arrays, dehydratedEntities);
                    if (entity is ReferenceEntity r && r.Reference is IDehydratedEntity d)
                    {
                        dehydratedEntities.Enqueue((r, d), d.Priority);
                    }
                    entities.Add(entity);
                }
                return StructureEntity.CreateWithList(s, entities);
            case EnumType e:
                return new EnumEntity(e, reader.ReadInt32());
            case PrimitiveType p:
                return p.DeserializeValue(reader);
            default: throw new NotSupportedException();
        };
    }
    static IEntity ReadAndDeserializeField(BinaryReader reader, StructureField field, Dictionary<(int block, int offset), ArrayEntity> arrays, PriorityQueue<(ReferenceEntity, IDehydratedEntity), int> dehydratedEntities)
    {
        if (!field.IsArray)
        {
            return field.PointerType switch
            {
                PointerTypes.Reference => ReadAndDeserializeType(reader, field.Type, arrays, dehydratedEntities),
                PointerTypes.Pointer => new ReferenceEntity(field.Type, new DehydratedPointerEntity(field.Type, reader.ReadInt16(), reader.ReadInt16())),
                PointerTypes.External => new ReferenceEntity(field.Type, new DehydratedRecordEntity(field.Type, reader.ReadInt32())),
                _ => throw new NotSupportedException()
            };
        }
        else
        {
            short blockId = reader.ReadInt16();
            short offset = reader.ReadInt16();
            int length = reader.ReadInt32();
            if(!arrays.TryGetValue((blockId, offset), out var arr))
            {
                arr = new ArrayEntity(field.Type);
                for (int i = 0; i < length; i++)
                {
                    var pointer = new DehydratedPointerEntity(field.Type, blockId, (short)(offset + i));
                    var reference = new ReferenceEntity(field.Type, pointer);
                    dehydratedEntities.Enqueue((reference, pointer), pointer.Priority);
                    arr.AddReference(reference);
                }
                arrays[(blockId, offset)] = arr;
            }
            return new ReferenceEntity(arr.Type, arr);
        }
    }
    static void ReadDataTypes(BinaryReader reader, BlurSerialization serialization)
    {
        for (int i = 0; i < serialization.DataTypesHeader.Length; i++)
        {
            serialization.TypeDefinitions.Add(DataTypeDefinition.Read(reader));
        }
        for (int i = 0; i < serialization.InheritenceHeader.Length; i++)
        {
            serialization.InheritenceDefinitions.Add(reader.ReadInt16());
            reader.ReadInt16();
        }
        for (int i = 0; i < serialization.DataFieldsHeader.Length; i++)
        {
            serialization.FieldDefinitions.Add(DataFieldDefinition.Read(reader));
        }

        serialization.TypeNames = new BlurEncoding().GetChars(reader.ReadBytes((int)serialization.TypeNamesHeader.Length));
    }

    static public void ReadRecords(BinaryReader reader, BlurSerialization serialization)
    {
        for (int i = 0; i < serialization.RecordsHeader.Length; i++)
        {
            serialization.RecordDefinitions.Add(RecordDefinition.Read(reader));
        }
        for (int i = 0; i < serialization.EntitiesHeader.Length; i++)
        {
            serialization.EntityDefinitions.Add(EntityDefinition.Read(reader));
        }
        for (int i = 0; i < serialization.BlocksHeader.Length; i++)
        {
            serialization.BlockDefinitions.Add(BlockDefinition.Read(reader));
        }
    }

    public void Serialize(Stream stream) => throw new NotImplementedException();
    public Stream Serialize()
    {
        var stream = new MemoryStream();
        Serialize(stream);
        return stream;
    }

}
