using BlurFormats.BinFile.Definitions;
using BlurFormats.BinFile.Entities;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile;
public sealed class BinBlock
{
    public List<DataType> DataTypes { get; private set; }

    public BinBlock()
    {
        DataTypes = new List<DataType>();
    }

    public void PrintTypes(TextWriter writeTo)
    {
        foreach (var type in DataTypes)
        {
            if(type.ReadType is 1 or 2)
            {
                if (type.ReadType == 2) writeTo.WriteLine("[Flags]");
                writeTo.WriteLine($"enum {type.Name} : {type.Unknown}");
                writeTo.WriteLine("{");
                foreach (var field in type.Fields)
                {
                    writeTo.WriteLine($"\t{field.Name},");
                }
                writeTo.WriteLine("}");
            }
            else
            {
                writeTo.WriteLine($"struct {type.Name} : {type.Unknown}");
                writeTo.WriteLine("{");
                foreach (var field in type.Fields)
                {
                    if(field.ReadType < 256)
                    {
                        if(field.ReadType >= 4)
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
                        if(field.ReadType == 259)
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

    public static BinBlock FromBytes(byte[] bytes)
    {
        var result =  new BinBlock();
        var reader = new Reader(bytes);
        var read = BinRead.FromBytes(ref reader);

        foreach (var dataType in read.DataTypeDefinitions)
        {
            result.DataTypes.Add(new DataType(read.StringsData.GetTerminatedStringAtOffset(dataType.StringOffset), dataType.Unknown, dataType.ReadType, dataType.DecodeType, dataType.Size));
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
        List<EntityBlock> entities = new();

        var block3Enumerator = read.Block3Definitions.GetEnumerator();
        foreach (var entityDefinition in read.EntityDefinitions)
        {
            if(entityDefinition.EntityName != ushort.MaxValue)
            {
                var entitySpecifics = read.EntityNameDefinitions[entityDefinition.EntityName];
                var entity = new EntityBlock();
                
                for (int i = 0; i < entitySpecifics.Length; i++)
                {
                    block3Enumerator.MoveNext();

                    var block = block3Enumerator.Current;
                    var readType = result.DataTypes[block.DataType];

                    var items = new List<IEntityData>();
                    for(int j = 0; j < block.Count; j++)
                    {
                        switch (block.Unknown2)
                        {
                            case 0:
                                var typeReader = reader.Subreader(readType.Size);
                                var entityData = ReadEntity(ref typeReader, readType, read.StringsData, result.DataTypes);
                                items.Add(entityData);
                                break;
                            case 1:
                                items.Add(new EntityData<DoublePointerData>(readType)
                                {
                                    Value = new DoublePointerData(reader.ReadUShort(), reader.ReadUShort()),
                                });
                                break;
                            case 2:
                                items.Add(new EntityData<PointerData>(readType)
                                {
                                    Value = new PointerData(reader.ReadUInt())
                                });
                                break;
                            default:
                                throw new Exception();
                        }
                    }
                    entity.Data.Add(new EntityData<List<IEntityData>>(readType)
                    {
                        Value = items
                    });
                }
                entity.ExtraBytes = reader.Read(entitySpecifics.Offset).ToArray();
                entities.Add(entity);
            }
        }

        return result;
    }
    public static IEntityData ReadEntity(ref Reader reader, DataField readField, string stringPool, IReadOnlyList<DataType> dataTypes)
    {
        IEntityData entityResult;
        switch (readField.ReadType)
        {
            case 0:
            case 1:
            case 2:
            case 3:
                entityResult = ReadEntity(ref reader, readField.DataType!, stringPool, dataTypes);
                entityResult.DataField = readField;
                break;
            case 4:
            case 5:
                entityResult = new EntityData<PointerData>(readField.DataType!){
                    Value = new PointerData(reader.ReadUInt()),
                    DataField = readField,
                };
                break;
            case 256:
            case 257:
            case 258:
            case 259:
            case 260:
            case 261:
                {
                    var fieldLength = reader.ReadUInt();
                    var pointer = reader.ReadUInt();
                    entityResult = new EntityData<ArrayPointerData>(readField.DataType!)
                    {
                        Value = new ArrayPointerData(fieldLength, pointer),
                        DataField = readField
                    };
                }
                break;
            case var c:
                throw new Exception($"Unknown read type {c}");
        }
        return entityResult;
    }
    public static IEntityData ReadEntity(ref Reader reader, DataType readType, string stringPool, IReadOnlyList<DataType> dataTypes)
    {
        IEntityData entityResult;
        switch (readType.ReadType)
        {
            case 0:
                entityResult = Decode(ref reader, readType);
                break;
            case 1 or 2:
                entityResult = Decode(ref reader, readType);
                break;
            case 3:
                var fields = new List<IEntityData>();
                foreach (var field in readType.Fields)
                {
                    reader.Seek(field.Offset);
                    fields.Add(ReadEntity(ref reader, field, stringPool, dataTypes));
                }
                entityResult = new EntityData<List<IEntityData>>(readType)
                {
                    Value = fields
                };
                break;
            case var c:
                throw new Exception($"Unknown read type {c}");
        }
        return entityResult;
    }
    public static IEntityData Decode(ref Reader reader, DataType readType) => readType.DecodeType switch
    {
        0 => new EntityData<EnumData>(readType) { Value = new EnumData(reader.ReadUInt())},
        1 => new EntityData<bool>(readType) { Value = reader.ReadInt() > 0 },
        2 => new EntityData<sbyte>(readType) { Value = (sbyte)reader.ReadInt() },
        3 => new EntityData<short>(readType) { Value = (short)reader.ReadInt() },
        4 => new EntityData<int>(readType) { Value = reader.ReadInt() },
        5 => new EntityData<long>(readType) { Value = reader.ReadLong() },
        6 => new EntityData<byte>(readType) { Value = (byte)reader.ReadInt() },
        7 => new EntityData<ushort>(readType) { Value = (ushort)reader.ReadInt() },
        8 => new EntityData<uint>(readType) { Value = reader.ReadUInt() },
        9 => new EntityData<ulong>(readType) { Value = reader.ReadULong() },
        10 => new EntityData<float>(readType) { Value = reader.ReadFloat() },
        11 => new EntityData<double>(readType) { Value = reader.ReadDouble() },
        12 => new EntityData<StringPointerData>(readType) { Value = new StringPointerData(reader.ReadUInt()) },
        14 => new EntityData<LocPointerData>(readType) { Value = new LocPointerData(reader.ReadUInt()) },
        var i => throw new Exception($"{i} is not yet a handled decode type.")
    };
}
