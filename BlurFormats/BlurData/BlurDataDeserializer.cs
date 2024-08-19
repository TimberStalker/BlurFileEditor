using BlurFormats.BlurData.Entities.Pointers;
using BlurFormats.BlurData.Entities;
using BlurFormats.BlurData.Types;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Net.WebSockets;
using BlurFormats.Serialization.Definitions;

namespace BlurFormats.BlurData;
internal ref struct BlurDataDeserializer
{
    static Encoding BlurEncoding = new BlurEncoding();
    BinaryReader r;
    List<DataType> types;
    List<BlurRecord> records;

    Stack<StringEntity> stringRequestsHydration;
    Stack<IHydrateableEntity> referenceRequestsHydration;
    Stack<IExternalHydrateableEntity> referenceRequestsExternalHydration;

    Header dataTypesHeader;
    Header inheritenceHeader;
    Header dataFieldsHeader;
    Header typeNamesHeader;
    Header recordsHeader;
    Header entitiesHeader;
    Header blocksHeader;
    Header dataHeader;

    public BlurDataDeserializer(Stream stream)
    {
        types = new List<DataType>();
        records = new List<BlurRecord>();

        stringRequestsHydration = new Stack<StringEntity>();
        referenceRequestsHydration = new Stack<IHydrateableEntity>();
        referenceRequestsExternalHydration = new Stack<IExternalHydrateableEntity>();

        r = new BinaryReader(stream, new BlurEncoding(), false);

        int format = r.ReadInt32();
        if (format != 0x464C534B) throw new Exception("Provided bytes are not of the KSLF(bin) format.");

        int magicNumber = r.ReadInt32();
        if (magicNumber != 2) throw new Exception($"Magic number is not correct. Should be '2' but instead is '{magicNumber}'.");

        ReadHeader(out dataTypesHeader);
        ReadHeader(out inheritenceHeader);
        ReadHeader(out dataFieldsHeader);
        ReadHeader(out typeNamesHeader);
        ReadHeader(out recordsHeader);
        ReadHeader(out entitiesHeader);
        ReadHeader(out blocksHeader);
        ReadHeader(out dataHeader);
    }

    public void ReadHeader(out Header header)
    {
        uint start = r.ReadUInt32();
        uint length = r.ReadUInt32();
        header = new Header(start, length);
    }

    public void ReadDataTypeDefinition(out DataTypeDefinition type)
    {
        short stringOffset = r.ReadInt16();
        short fieldCount = r.ReadInt16();
        short hasBase = r.ReadInt16();
        short structureType = r.ReadInt16();
        short primitiveType = r.ReadInt16();
        short size = r.ReadInt16();

        type = new DataTypeDefinition(stringOffset, fieldCount, hasBase, structureType, primitiveType, size);
    }
    public void ReadDataFieldDefinition(out DataFieldDefinition field)
    {
        short nameOffset = r.ReadInt16();
        short baseType = r.ReadInt16();
        short offset = r.ReadInt16();
        byte fieldType = r.ReadByte();
        byte isArray = r.ReadByte();

        field = new DataFieldDefinition(nameOffset, baseType, offset, fieldType, isArray);
    }

    public BlurData ToBlurData() 
    {
        return new BlurData(types, records);
    }
    public void DeserializeDataTypes()
    {
        Span<DataTypeDefinition> typeDefinitions = stackalloc DataTypeDefinition[(int)dataTypesHeader.Length];
        Span<int> inheritenceDefinitions = stackalloc int[(int)inheritenceHeader.Length];
        Span<DataFieldDefinition> fieldDefinitions = stackalloc DataFieldDefinition[(int)dataFieldsHeader.Length];
        for (int i = 0; i < dataTypesHeader.Length; i++)
        {
            ReadDataTypeDefinition(out typeDefinitions[i]);
        }
        for (int i = 0; i < inheritenceHeader.Length; i++)
        {
            inheritenceDefinitions[i] = r.ReadInt32();
        }
        for (int i = 0; i < dataFieldsHeader.Length; i++)
        {
            ReadDataFieldDefinition(out fieldDefinitions[i]);
        }
        char[] typeNames = new BlurEncoding().GetChars(r.ReadBytes((int)typeNamesHeader.Length));

        foreach (var typeDefinition in typeDefinitions)
        {
            var name = typeNames.GetTerminatedString(typeDefinition.StringOffset);
            var structureType = (StructureType)typeDefinition.StructureType;
            var primitiveType = (PrimitiveType)typeDefinition.PrimitiveType;

            var newType = new DataType(name, structureType, primitiveType);
            switch(name)
            {
                case "RP_GraphPoint":
                    newType.FormatString = @"({x}, {y})";
                    break;
                case "Vec3":
                    newType.FormatString = @"({vx}, {vy}, {vz})";
                    break;
            }
            types.Add(newType);
        }

        var inheritenceEnumerator = inheritenceDefinitions.GetEnumerator();
        var fieldsEnumerator = fieldDefinitions.GetEnumerator();
        for(int i = 0; i < typeDefinitions.Length; i++)
        {
            var definition = typeDefinitions[i];
            var type = types[i];

            if (definition.HasBase == 1)
            {
                inheritenceEnumerator.MoveNext();
                type.Base = types[inheritenceEnumerator.Current];
            }
            else if (definition.HasBase > 1) throw new NotImplementedException();

            for (int j = 0; j < definition.FieldCount; j++)
            {
                fieldsEnumerator.MoveNext();
                var fieldDefinition = fieldsEnumerator.Current;

                var name = typeNames.GetTerminatedString(fieldDefinition.NameOffset);
                var fieldType = (FieldType)fieldDefinition.FieldType;
                var baseType = fieldDefinition.BaseType != -1 ? types[fieldDefinition.BaseType] : null;

                var field = new DataField(name, fieldType, baseType);
                type.Fields.Add(field);
            }
        }
    }

    public void DeserializeRecords()
    {
        Span<RecordDefinition> recordDefinitions = stackalloc RecordDefinition[(int)recordsHeader.Length];
        Span<EntityDefinition> entityDefinitions = stackalloc EntityDefinition[(int)entitiesHeader.Length];
        Span<BlockDefinition> blockDefinitions = new BlockDefinition[blocksHeader.Length];

        for (int i = 0; i < recordsHeader.Length; i++)
        {
            recordDefinitions[i] = RecordDefinition.Read(r);
        }
        for (int i = 0; i < entitiesHeader.Length; i++)
        {
            entityDefinitions[i] = EntityDefinition.Read(r);
        }
        for (int i = 0; i < blocksHeader.Length; i++)
        {
            blockDefinitions[i] = BlockDefinition.Read(r);
        }

        if (r.BaseStream.Position % 4 != 0)
        {
            r.BaseStream.Seek(4 - r.BaseStream.Position % 4, SeekOrigin.Current);
        }

        int currentBlock = 0;
        for (int k = 0; k < recordDefinitions.Length; k++)
        {
            ref RecordDefinition record = ref recordDefinitions[k];
            if (record.Entity == -1)
            {
                records.Add(new BlurRecord(record.ID, new NullEntity(types[record.BaseType])));
                continue;
            }

            var recordInfo = entityDefinitions[record.Entity];

            RecordHeap heap = new RecordHeap();
            List<EntityBlock> entityBlocks = DeserializeRecord(blockDefinitions, ref currentBlock, in recordInfo, heap);

            records.Add(new BlurRecord(record.ID, entityBlocks[0].Value[0], heap));
        }

        //var recordPointerDataSource = new PointerDataSource(records);

        while (referenceRequestsExternalHydration.Count > 0)
        {
            var external = referenceRequestsExternalHydration.Pop();
            //external.Hydrate(records);
        }
    }

    private List<EntityBlock> DeserializeRecord(in ReadOnlySpan<BlockDefinition> blockDefinitions, ref int currentBlock, in EntityDefinition recordInfo, RecordHeap heap)
    {
        List<EntityBlock> entityBlocks = new List<EntityBlock>(recordInfo.Length);
        for (int i = 0; i < recordInfo.Length; i++)
        {
            entityBlocks.Add(DeserializeBlock(blockDefinitions[currentBlock++]));
        }
        
        var strings = new BlurEncoding().GetChars(r.ReadBytes(recordInfo.NamesLength));

        var pointerDataSource = new PointerDataSource(strings, entityBlocks);
        
        while (stringRequestsHydration.Count > 0)
        {
            var stringEntity = stringRequestsHydration.Pop();
            stringEntity.GetStringFromOffset(strings);
        }

        while (referenceRequestsHydration.Count > 0)
        {
            var referenceEntity = referenceRequestsHydration.Pop();
            referenceEntity.Hydrate(heap, entityBlocks);
        }

        return entityBlocks;
    }
    
    private EntityBlock DeserializeBlock(BlockDefinition entityBlock)
    {
        var blockType = types[entityBlock.DataType];
        var entities = new EntityBlock(blockType);
        for (int j = 0; j < entityBlock.Count; j++)
        {
            var newEntity = entityBlock.PointerType switch
            {
                0 => DeserializeType(blockType),
                1 => new ForwardEntity(blockType, r.ReadInt16(), r.ReadInt16()),
                2 => new ExternalForwardEntity(blockType, r.ReadInt32()),
                _ => throw new NotImplementedException()
            };
            entities.Value.Add(newEntity);
        }

        return entities;
    }

    IEntity DeserializeField(DataField field)
    {
        switch (field.FieldType)
        {
            case FieldType.Primitive:
            case FieldType.Enum:
            case FieldType.FlagsEnum:
            case FieldType.Struct:
                var entityResult = DeserializeType(field.DataType!);
                return entityResult;
            case FieldType.Pointer:
                var pointerEntity = new ReferenceEntity(field.DataType!, r.ReadInt16(), r.ReadInt16());
                referenceRequestsHydration.Push(pointerEntity);
                return pointerEntity;
            case FieldType.ExternalPointer:
                var extPointer = new ExternalReferenceEntity(field.DataType!, r.ReadInt32());
                referenceRequestsExternalHydration.Push(extPointer);
                return extPointer;
            case FieldType.PrimitiveArray:
            case FieldType.EnumArray:
            case FieldType.StructArray:
            case FieldType.PointerArray:
                {
                    var pointer = r.ReadInt16();
                    var modifier = r.ReadInt16();
                    var fieldLength = r.ReadInt32();
                    var arrEntity = new ArrayEntity(field.DataType!, pointer, modifier, fieldLength);
                    referenceRequestsHydration.Push(arrEntity);
                    return arrEntity;
                }
            case FieldType.ExternalArray:
                {
                    var pointer = r.ReadInt16();
                    var modifier = r.ReadInt16();
                    var fieldLength = r.ReadInt32();
                    var extArrayEntity = new ExternalArrayEntity(field.DataType!, pointer, modifier, fieldLength);
                    referenceRequestsExternalHydration.Push(extArrayEntity);
                    return extArrayEntity;
                }
            case var c:
                throw new Exception($"Unknown read type {c}");
        }
    }

    IEntity DeserializeType(DataType type)
    {
        switch (type.StructureType)
        {
            case StructureType.Primitive:
                var primitive = DeserializePrimitive(type);
                if(primitive is StringEntity s) 
                { 
                    stringRequestsHydration.Push(s);
                }
                return primitive;
            case StructureType.Enum:
                return new EnumEntity(type, r.ReadInt32());
            case StructureType.Flags:
                return new FlagsEnumEntity(type, r.ReadInt32());
            case StructureType.Struct:
                ObjectEntity entityResult;
                if (type.Base is not null)
                {
                    entityResult = (ObjectEntity)DeserializeType(type.Base);
                    entityResult.Type = type;
                }
                else
                {
                    entityResult = new ObjectEntity(type);
                }
                int baseFieldCount = entityResult.Fields.Count;
                for (int i = 0; i < type.Fields.Count; i++)
                {
                    DataField? field = type.Fields[i];
                    var fieldValue = DeserializeField(field);

                    entityResult.Fields.Add(new ObjectEntityItem(field, fieldValue));
                }
                return entityResult;
            default:
                return new NullEntity(type);
        }
    }
    IEntity DeserializePrimitive(DataType dataType) => dataType.PrimitiveType switch
    {
        PrimitiveType.None => new NullEntity(dataType),
        PrimitiveType.Bool => new PrimitiveEntity<bool>(dataType, r.ReadInt32() > 0),
        PrimitiveType.Byte => new PrimitiveEntity<sbyte>(dataType, (sbyte)r.ReadInt32()),
        PrimitiveType.Short => new PrimitiveEntity<short>(dataType, (short)r.ReadInt32()),
        PrimitiveType.Integer => new PrimitiveEntity<int>(dataType, r.ReadInt32()),
        PrimitiveType.Long => new PrimitiveEntity<long>(dataType, r.ReadInt64()),
        PrimitiveType.UByte => new PrimitiveEntity<byte>(dataType, (byte)r.ReadInt32()),
        PrimitiveType.UShort => new PrimitiveEntity<ushort>(dataType, (ushort)r.ReadInt32()),
        PrimitiveType.UInt => new PrimitiveEntity<uint>(dataType, r.ReadUInt32()),
        PrimitiveType.ULong => new PrimitiveEntity<ulong>(dataType, r.ReadUInt64()),
        PrimitiveType.Float => new PrimitiveEntity<float>(dataType, r.ReadSingle()),
        PrimitiveType.Double => new PrimitiveEntity<double>(dataType, r.ReadDouble()),
        PrimitiveType.String => new StringEntity(dataType, r.ReadInt32()),
        PrimitiveType.Localization => new LocPointerEntity(dataType, r.ReadInt32()),
        var i => throw new Exception($"{i} is not yet a handled decode type.")
    };

    static void HydrateEntityItem(IEntity parent, int index, PointerDataSource dataSource)
    {
        if (parent is ObjectEntity o)
        {
            if(o.Fields[index].Value is not NullEntity)
                o.Fields[index].Value = ((IPointerEntity)o.Fields[index].Value).GetReplacement(dataSource);
        } else if (parent is EntityBlock b)
        {
            if(b.Value[index] is not NullEntity)
                b.Value[index] = ((IPointerEntity)b.Value[index]).GetReplacement(dataSource);
        }
    }
}
