using BlurFormats.BlurData.Entities;
using BlurFormats.BlurData.Entities.Pointers;
using BlurFormats.BlurData.Read;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData;
public class BlurData
{
    public ObservableCollection<DataType> DataTypes { get; } = new ObservableCollection<DataType>();
    public ObservableCollection<Entity> Entities { get; } = new ObservableCollection<Entity>();

    public void PrintTypes(TextWriter writeTo)
    {
        foreach (var type in DataTypes)
        {
            if (type.ReadType is ReadType.Enum or ReadType.FlagsEnum)
            {
                if ((int)type.ReadType == 2) writeTo.WriteLine("[Flags]");
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
                if(type.Fields.Count == 0) writeTo.Write("external ");
                writeTo.Write($"struct {type.Name}");
                if(type.Parent is not null)
                {
                    writeTo.Write($" : {type.Parent.Name}");
                }
                writeTo.WriteLine("\n{");
                foreach (var field in type.Fields)
                {
                    if ((int)field.ReadType < 256)
                    {
                        if ((int)field.ReadType >= 4)
                        {
                            writeTo.WriteLine($"\t{field.DataType!.Name}* {field.Name};");
                        }
                        else
                        {
                            writeTo.WriteLine($"\t{field.DataType!.Name} {field.Name};");
                        }
                    }
                    else
                    {
                        if ((int)field.ReadType == 259)
                        {
                            writeTo.WriteLine($"\t{field.DataType!.Name}*[] {field.Name};");
                        }
                        else
                        {
                            writeTo.WriteLine($"\t{field.DataType!.Name}[] {field.Name};");
                        }
                    }
                }
                writeTo.WriteLine("}");
            }
        }
    }
    //public static BlurData Deserialize(byte[] bytes)
    //{
    //    var reader = new Reader(bytes);
    //    string format = reader.ReadString(4);
    //    if (format != "KSLF") throw new Exception("Provided bytes are not of the KSLF(bin) format.");

    //    int magicNumber = reader.ReadInt();
    //    if (magicNumber != 2) throw new Exception($"Magic number is not correct. Should be '2' but instead is '{magicNumber}'.");

    //    Header dataTypesHeader = reader.Read<Header>();
    //    Header inheritenceHeader = reader.Read<Header>();
    //    Header dataFieldsHeader = reader.Read<Header>();
    //    Header typeNamesHeader = reader.Read<Header>();
    //    Header recordssHeader = reader.Read<Header>();
    //    Header entityNamesHeader = reader.Read<Header>();
    //    Header block3Header = reader.Read<Header>();
    //    Header dataBlockHeader = reader.Read<Header>();

    //    var dataTypes = DeserializeDataTypes(ref reader, dataTypesHeader, inheritenceHeader, dataFieldsHeader, typeNamesHeader);
    //}

    //public static List<DataType> DeserializeDataTypes(ref Reader reader, Header dataTypesHeader, Header inheritenceHeader, Header dataFieldsHeader, Header typeNamesHeader)
    //{
    //    List<DataTypeDefinition> typeDefinitions = new List<DataTypeDefinition>();
    //    List<int> inheritenceDefinitions = new List<int>();
    //    List<DatafieldDefinition> fieldDefinitions = new List<DatafieldDefinition>();

    //    for (int i = 0; i < dataTypesHeader.Length; i++)
    //    {
    //        typeDefinitions.Add(reader.Read<DataTypeDefinition>());
    //    }
    //    for (int i = 0; i < inheritenceHeader.Length; i++)
    //    {
    //        inheritenceDefinitions.Add(reader.ReadInt());
    //    }
    //    for (int i = 0; i < dataFieldsHeader.Length; i++)
    //    {
    //        fieldDefinitions.Add(reader.Read<DatafieldDefinition>());
    //    }

    //    string typeNames = reader.ReadStringDecrypted(typeNamesHeader.Length);

    //    foreach (var typeDefinition in typeDefinitions)
    //    {
    //        var name = typeNames.GetTerminatedStringAtOffset(typeDefinition.StringOffset);
    //    }
    //}

    public static BlurData FromBytes(byte[] bytes)
    {
        var reader = new Reader(bytes);
        var read = DataRead.FromBytes(ref reader);

        var result = new BlurData();

        foreach (var dataType in read.DataTypeDefinitions)
        {
            result.DataTypes.Add(new DataType(read.StringsData.GetTerminatedStringAtOffset(dataType.StringOffset), dataType.StructureType, dataType.PrimitiveType, dataType.Size));
        }

        int parentIndex = 0;
        int typeIndex = 0;
        foreach (var dataType in result.DataTypes)
        {
            if(read.DataTypeDefinitions[typeIndex].HasParent > 0)
            {
                dataType.Parent = result.DataTypes[read.UnknownDefinitions[parentIndex]];
                parentIndex++;
            }
            typeIndex++;
        }

        List<DataField> dataFields = new();

        foreach (var field in read.DataFieldDefinitions)
        {
            dataFields.Add(new DataField(read.StringsData.GetTerminatedStringAtOffset(field.StringOffset), field.ReadType, field.Offset, field.StructureType != ushort.MaxValue ? result.DataTypes[field.StructureType] : null));
        }

        int offset = 0;
        for (int i = 0; i < result.DataTypes.Count; i++)
        {
            result.DataTypes[i].Fields.AddRange(dataFields.Skip(offset).Take(read.DataTypeDefinitions[i].ItemCount));
            offset += read.DataTypeDefinitions[i].ItemCount;
        }

        var entitiesBlocks = new List<EntityBlock>();
        var block3Enumerator = read.Block3Definitions.GetEnumerator();
        foreach (var entityDefinition in read.EntityDefinitions)
        {
            if (entityDefinition.EntitySpecifics != -1)
            {
                var entitySpecifics = read.EntityNameDefinitions[entityDefinition.EntitySpecifics];
                EntityBlock entity = new EntityBlock();
                for (int i = 0; i < entitySpecifics.Length; i++)
                {
                    block3Enumerator.MoveNext();

                    var block = block3Enumerator.Current;
                    var readType = result.DataTypes[block.DataType];
                    List<Entity> blockValueEntities = new List<Entity>();
                    for (int j = 0; j < block.Count; j++)
                    {
                        switch (block.PointerType)
                        {
                            case 0:
                                //var classReader = reader.Subreader(readType.Size);
                                var entityData = ReadEntity(ref reader, readType, read.StringsData, result.DataTypes);
                                blockValueEntities.Add(entityData);
                                break;
                            case 1:
                                blockValueEntities.Add(new DoublePointerEntity(readType, (reader.ReadUShort(), reader.ReadUShort())));
                                break;
                            case 2:
                                blockValueEntities.Add(new PointerEntity(readType, reader.ReadInt()));
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    entity.Entities.Add(blockValueEntities);
                }
                entity.ExtraBytes = reader.Read(entitySpecifics.Offset).ToArray();
                entitiesBlocks.Add(entity);
            }
        }
        foreach (var entityBlock in entitiesBlocks)
        {
            foreach (var entities in entityBlock.Entities)
            {
                foreach (var entity in entities)
                {
                    FillEntityFields(new PointerDataSource(entityBlock, entitiesBlocks, Reader.Decrypt(entityBlock.ExtraBytes)), entity);
                }
            }
            result.Entities.Add(entityBlock.Entities[0][0]);
        }
        return result;
    }
    private static void FillEntityFields(PointerDataSource pointerDataSource, Entity entity)
    {
        if(entity is ObjectEntity o)
        {
            for(int i = 0; i < o.Fields.Count; i++)
            {
                if(o.Fields[i] is IPointerEntity p)
                {
                    o.Fields[i] = p.GetReplacement(pointerDataSource);
                }
                else
                {
                    FillEntityFields(pointerDataSource, o.Fields[i]);
                }
            }
        } else if (entity is ArrayEntity a)
        {
            for(int i = 0; i < a.Value.Count; i++)
            {
                if (a.Value[i] is IPointerEntity p)
                {
                    a.Value[i] = p.GetReplacement(pointerDataSource);
                }
                else
                {
                    FillEntityFields(pointerDataSource, a.Value[i]);
                }
            }
        }
    }
    public static Entity ReadEntity(ref Reader reader, DataField readField, string stringPool, IReadOnlyList<DataType> dataTypes)
    {
        Entity entityResult;
        switch (readField.ReadType)
        {
            case ReadType.Primitive:
            case ReadType.Enum:
            case ReadType.FlagsEnum:
            case ReadType.Struct:
                entityResult = ReadEntity(ref reader, readField.DataType!, stringPool, dataTypes);
                entityResult.DataField = readField;
                break;
            case ReadType.Pointer:
                entityResult = new PointerEntity(readField.DataType!, reader.ReadInt())
                {
                    DataField = readField,
                };
                break;
            case ReadType.ExternalPointer:
                entityResult = new ExternalPointerEntity(readField.DataType!, reader.ReadInt())
                {
                    DataField = readField,
                };
                break;
            case ReadType.PrimitiveArray:
            case ReadType.EnumArray:
            case ReadType.StructArray:
            case ReadType.PointerArray:
                {
                    var pointer = reader.ReadShort();
                    var modifier = reader.ReadShort();
                    var fieldLength = reader.ReadInt();
                    entityResult = new ArrayPointerEntity(readField.DataType!, pointer, modifier, fieldLength)
                    {
                        DataField = readField
                    };
                }
                break;
            case ReadType.ExternalArray:
                {
                    var pointer = reader.ReadShort();
                    var modifier = reader.ReadShort();
                    var fieldLength = reader.ReadInt();
                    entityResult = new ExternalArrayPointerEntity(readField.DataType!, pointer, modifier, fieldLength)
                    {
                        DataField = readField
                    };
                }
                break;
            case var c:
                throw new Exception($"Unknown read type {c}");
        }
        return entityResult;
    }
    public static void AddEntityParentFields(ObjectEntity objectEntity, ref Reader reader, DataType readType, string stringPool, IReadOnlyList<DataType> dataTypes)
    {
        if (readType.Parent is not null) AddEntityParentFields(objectEntity, ref reader, readType.Parent, stringPool, dataTypes);
        foreach (var field in readType.Fields)
        {
            objectEntity.Fields.Add(ReadEntity(ref reader, field, stringPool, dataTypes));
        }
    }
    public static Entity ReadEntity(ref Reader reader, DataType readType, string stringPool, IReadOnlyList<DataType> dataTypes)
    {
        Entity entityResult;
        switch (readType.ReadType)
        {
            case ReadType.Primitive:
                int start = reader.Position;
                entityResult = Decode(ref reader, readType);
                entityResult.Range = start..reader.Position;
                break;
            case ReadType.Enum:
                entityResult = new EnumEntity(readType, reader.ReadInt());
                break;
            case ReadType.FlagsEnum:
                entityResult = new FlagsEnumEntity(readType, reader.ReadInt());
                break;
            case ReadType.Struct:
                var objectEntity = new ObjectEntity(readType);

                if (readType.Parent is not null) AddEntityParentFields(objectEntity, ref reader, readType.Parent, stringPool, dataTypes);
                foreach (var field in readType.Fields)
                {
                    objectEntity.Fields.Add(ReadEntity(ref reader, field, stringPool, dataTypes));
                }
                entityResult = objectEntity;
                break;
            case var c:
                throw new Exception($"Unknown read type {c}");
        }
        return entityResult;
    }
    public static Entity Decode(ref Reader reader, DataType readType) => readType.DecodeType switch
    {
        DecodeType.None => new NullEntity(readType),
        DecodeType.Bool => new PrimitiveEntity<bool>(readType, reader.ReadInt() > 0),
        DecodeType.Byte => new PrimitiveEntity<sbyte>(readType, (sbyte)reader.ReadInt()),
        DecodeType.Short => new PrimitiveEntity<short>(readType, (short)reader.ReadInt()),
        DecodeType.Integer => new PrimitiveEntity<int>(readType, reader.ReadInt()),
        DecodeType.Long => new PrimitiveEntity<long>(readType, reader.ReadLong()),
        DecodeType.UByte => new PrimitiveEntity<byte>(readType, (byte)reader.ReadInt()),
        DecodeType.UShort => new PrimitiveEntity<ushort>(readType, (ushort)reader.ReadInt()),
        DecodeType.UInt => new PrimitiveEntity<uint>(readType, reader.ReadUInt()),
        DecodeType.ULong => new PrimitiveEntity<ulong>(readType, reader.ReadULong()),
        DecodeType.Float => new PrimitiveEntity<float>(readType, reader.ReadFloat()),
        DecodeType.Double => new PrimitiveEntity<double>(readType, reader.ReadDouble()),
        DecodeType.String => new StringPointerEntity(readType, reader.ReadInt()),
        DecodeType.Localaization => new LocPointerEntity(readType, reader.ReadInt()),
        var i => throw new Exception($"{i} is not yet a handled decode type.")
    };
}
