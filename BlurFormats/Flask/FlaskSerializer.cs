using BlurFormats.Serialization.Definitions;
using BlurFormats.Serialization.Entities;
using BlurFormats.Serialization.Entities.Dehydrated;
using BlurFormats.Serialization.Types;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Serialization;
public partial class FlaskSerializer
{
    const int HeaderSize = 72;
    public static void Serialize(List<SerializationType> types, List<SerializationRecord> records, string fileName)
    {
        using var stream = File.Open(fileName, FileMode.Create, FileAccess.Write);
        Serialize(types, records, stream);
    }
    public static void Serialize(List<SerializationType> types, List<SerializationRecord> records, Stream stream)
    {
        using var writer = new BinaryWriter(stream);
        var serialization = new BlurSerialization();
        writer.Write(Encoding.ASCII.GetBytes("KSLF"));
        writer.Write(2);

        uint offset = 72;

        using var typesStream = new MemoryStream();
        SerializeTypes(ref offset, serialization, types, typesStream);
        using var recordsStream = new MemoryStream();
        SerializeRecords(ref offset, serialization, types, records, recordsStream);

        serialization.DataTypesHeader.Write(writer);
        serialization.InheritenceHeader.Write(writer);
        serialization.DataFieldsHeader.Write(writer);
        serialization.TypeNamesHeader.Write(writer);
        serialization.RecordsHeader.Write(writer);
        serialization.EntitiesHeader.Write(writer);
        serialization.BlocksHeader.Write(writer);
        serialization.DataHeader.Write(writer);

        typesStream.Position = 0;
        typesStream.CopyTo(stream);
        recordsStream.Position = 0;
        recordsStream.CopyTo(stream);
    }
    public static void SerializeRecords(ref uint offset, BlurSerialization serialization, List<SerializationType> types, List<SerializationRecord> records, Stream stream)
    {
        using var recordStream = new MemoryStream();
        using var entitiesStream = new MemoryStream();
        using var blocksStream = new MemoryStream();
        using var dataStream = new MemoryStream();
        
        using var recordWriter = new BinaryWriter(recordStream);
        using var entitiesWriter = new BinaryWriter(entitiesStream);
        using var blocksWriter = new BinaryWriter(blocksStream);
        using var dataWriter = new BinaryWriter(dataStream);

        int recordCount = 0;
        int entityCount = 0;
        int blockCount = 0;

        foreach (var record in records)
        {
            recordWriter.Write(record.ID);
            recordWriter.Write((short)types.IndexOf(record.Entity.Type));
            if(record.Entity is ExternalRecordReferenceEntity)
            {
                recordWriter.Write((short)-1);
            }
            else
            {
                recordWriter.Write((short)entityCount);
                StringIndexer stringIndexer = new StringIndexer();
                
                FlaskSerializerBlockDictionary references = new();
                HashSet<ArrayEntity> dehydratedArrays = new HashSet<ArrayEntity>();
                references[(record.Entity.Type, 0)].Add(record.Entity);

                DehydrateEntity(record.Entity, stringIndexer, records, dehydratedArrays, references);

                int blockStart = blockCount;
                int dataSizeStart = (int)dataStream.Length;

                foreach (var (type, pointerType, entities) in references)
                {
                    if (entities.Count == 0) continue;
                    foreach (var item in entities)
                    {
                        item.Serialize(dataWriter);
                    }
                    int dataTypeIndex = types.IndexOf(type);

                    blocksWriter.Write((short)dataTypeIndex);
                    blocksWriter.Write((short)entities.Count);
                    blocksWriter.Write((short)pointerType);

                    blockCount++;
                }
                var nameBytes = new BlurEncoding().GetBytes(stringIndexer.ToString());

                entitiesWriter.Write(recordCount);
                entitiesWriter.Write(blockCount - blockStart);
                entitiesWriter.Write((int)dataStream.Length - dataSizeStart);
                
                dataWriter.Write(nameBytes);
                entitiesWriter.Write(nameBytes.Length);

                entityCount++;
            }


            recordCount++;
        }

        recordStream.Position = 0;
        entitiesStream.Position = 0;
        blocksStream.Position = 0;
        dataStream.Position = 0;

        serialization.RecordsHeader = new Header(offset, (uint)recordCount);
        recordStream.CopyTo(stream);
        offset += (uint)recordStream.Length;

        serialization.EntitiesHeader = new Header(offset, (uint)entityCount);
        entitiesStream.CopyTo(stream);
        offset += (uint)entitiesStream.Length;

        serialization.BlocksHeader = new Header(offset, (uint)blockCount);
        blocksStream.CopyTo(stream);
        offset += (uint)blocksStream.Length;
        while(offset % 4 != 0)
        {
            stream.WriteByte(0);
            offset++;
        }
        serialization.DataHeader = new Header(offset, (uint)dataStream.Length);
        dataStream.CopyTo(stream);
        offset += (uint)dataStream.Length;

    }
    public static void UpdateBlockPositions(IEntity entity, FlaskSerializerBlockDictionary references)
    {

    }
    public static void DehydrateEntity(IEntity entity, StringIndexer stringIndexer, List<SerializationRecord> records, HashSet<ArrayEntity> dehydratedArrays, FlaskSerializerBlockDictionary references)
    {
        if (entity is ReferenceEntity r)
        {
            DehydrateReference(r, stringIndexer, records, dehydratedArrays, references);
        }
        else if (entity is StructureEntity s)
        {
            foreach (var child in s.Values)
            {
                DehydrateEntity(child, stringIndexer, records, dehydratedArrays, references);
            }
        }
    }
    public static void DehydrateReference(ReferenceEntity reference, StringIndexer stringIndexer, List<SerializationRecord> records, HashSet<ArrayEntity> dehydratedArrays, FlaskSerializerBlockDictionary references)
    {
        if (reference.Reference is PrimitiveEntity p && p.Type.Primitive == PrimitiveType.Primitives.String)
        {
            reference.SetReference(new DehydratedStringEntity(p.Type, stringIndexer.GetIndex((string)reference.Value)));
        }
        else if (reference.Reference is RecordReferenceEntity rr)
        {
            var dehydrated = new DehydratedRecordEntity(rr.Type, records.IndexOf(rr.Record));

            reference.SetReference(dehydrated);
        }
        else if (reference.Reference is NullEntity)
        {
            reference.SetReference(new DehydratedPointerEntity(reference.Reference.Type, -1, -1));
        }
        else if (reference.Reference is ArrayEntity a)
        {
            DehydrateArray(a, stringIndexer, records, dehydratedArrays, references);
        }
        else
        {
            var entites = references[(reference.Reference.Type, 0)];
            
            if (!entites.Contains(reference.Reference))
            {
                entites.Add(reference.Reference);
                DehydrateEntity(reference.Reference, stringIndexer, records, dehydratedArrays, references);
            }

            //int block = references.IndexOf((reference.Reference.Type, 0));
            int offset = entites.IndexOf(reference.Reference);

            var pointer = new DehydratedBlockResolvingPointerEntity(reference.Reference.Type, () => references.BlockIndexOf(reference.Reference), (short)offset);
            reference.SetReference(pointer);
        }
    }

    private static void DehydrateArray(ArrayEntity array, StringIndexer stringIndexer, List<SerializationRecord> records, HashSet<ArrayEntity> dehydratedArrays, FlaskSerializerBlockDictionary references)
    {
        if (!dehydratedArrays.Add(array)) return;
        bool hasEntitesAlready = array.Entities.Any(e => references.Contains(e.Reference));
        bool typeMatches = !array.Entities.Any(e => e.Type != e.Reference.Type);
        Queue<IEntity> dehydrateQueue = new Queue<IEntity>();
        foreach (var child in array.Entities)
        {
            if (child.Reference is RecordReferenceEntity rre)
            {
                var entites = references[(rre.Type, PointerTypes.External)];

                var dehydratedRecord = new DehydratedRecordEntity(rre.Type, records.IndexOf(rre.Record));
                entites.Add(dehydratedRecord);

                //int block = references.IndexOf((rre.Type, PointerTypes.External));
                int offset = entites.IndexOf(dehydratedRecord);

                var pointer = new DehydratedBlockResolvingPointerEntity(rre.Type, () => references.BlockIndexOf(rre), (short)offset);
                child.SetReference(pointer);
            }
            else if (child.Reference is NullEntity)
            {
                child.SetReference(new DehydratedPointerEntity(child.Reference.Type, -1, -1));
            }
            else
            {
                var entites = references[(child.Reference.Type, PointerTypes.Reference)];
                if (!entites.Contains(child.Reference))
                {
                    entites.Add(child.Reference);
                    dehydrateQueue.Enqueue(child.Reference);
                }

                //int block = references.IndexOf((child.Reference.Type, 0));
                int offset = entites.IndexOf(child.Reference);

                var pointer = new DehydratedBlockResolvingPointerEntity(child.Reference.Type, () => references.BlockIndexOf(child.Reference), (short)offset);

                if(!hasEntitesAlready && typeMatches)
                {
                    child.SetReference(pointer);
                    continue;
                }

                var refEntites = references[(child.Type, PointerTypes.Pointer)];

                refEntites.Add(pointer);

                //block = references.IndexOf((child.Type, PointerTypes.Pointer));
                offset = refEntites.Count - 1;
                pointer = new DehydratedBlockResolvingPointerEntity(child.Type, () => references.BlockIndexOf(child), (short)offset);

                child.SetReference(pointer);
            }
        }
        while(dehydrateQueue.Count > 0)
        {
            DehydrateEntity(dehydrateQueue.Dequeue(), stringIndexer, records, dehydratedArrays, references);
        }
    }

    public static void SerializeTypes(ref uint offset, BlurSerialization serialization, List<SerializationType> types, Stream stream)
    {
        var writer = new BinaryWriter(stream);

        StringIndexer stringIndexer = new StringIndexer();

        using var typeStream = new MemoryStream();
        using var inheritenceStream = new MemoryStream();
        using var fieldStream = new MemoryStream();

        using var typeWriter = new BinaryWriter(typeStream);
        using var inheritenceWriter = new BinaryWriter(inheritenceStream);
        using var fieldWriter = new BinaryWriter(fieldStream);

        for (int i = 0; i < types.Count; i++)
        {
            var type = types[i];

            typeWriter.Write((short)stringIndexer.GetIndex(type.Name));

            switch(type)
            {
                case StructureType s:

                    typeWriter.Write((short)s.FieldCount);
                    int fieldOffset = s.Base?.Size ?? 0;
                    for (int j = 0; j < s.Fields.Count; j++)
                    {
                        var field = s.Fields[j];

                        fieldWriter.Write((short)stringIndexer.GetIndex(field.Name));
                        fieldWriter.Write((short)types.IndexOf(field.Type));
                        fieldWriter.Write((short)fieldOffset);
                        fieldOffset += field.Size;
                        int ft = field.PointerType switch
                        {
                            PointerTypes.Reference => field.Type switch
                            {
                                PrimitiveType _ => 0,
                                EnumType e => e.IsFlags ? 2 : 1,
                                StructureType _ => 3,
                                _ => throw new NotSupportedException()
                            },
                            PointerTypes.Pointer => 4,
                            PointerTypes.External => 5,
                            _ => throw new NotSupportedException(),
                        };
                        fieldWriter.Write((byte)ft);
                        fieldWriter.Write((byte)(field.IsArray ? 1 : 0));
                    }
                    if (s.Base is not null)
                    {
                        typeWriter.Write((short)1);
                        inheritenceWriter.Write(types.IndexOf(s.Base));
                    }
                    else
                    {
                        typeWriter.Write((short)0);
                    }
                    typeWriter.Write((short)3);
                    typeWriter.Write((short)0);
                    typeWriter.Write((short)type.Size);
                    break;
                case PrimitiveType p:
                    typeWriter.Write((short)0);
                    typeWriter.Write((short)0);
                    typeWriter.Write((short)0);
                    typeWriter.Write((short)p.Primitive);
                    typeWriter.Write((short)4);
                    break;
                case EnumType e:
                    typeWriter.Write((short)e.Values.Count); 
                    typeWriter.Write((short)0);
                    for (int j = 0; j < e.Values.Count; j++)
                    {
                        var name = e.Values[j];

                        fieldWriter.Write((short)stringIndexer.GetIndex(name));
                        fieldWriter.Write((short)-1);
                        fieldWriter.Write((short)0);
                        fieldWriter.Write((short)6);
                    }
                    typeWriter.Write((short)(e.IsFlags ? 2 : 1));
                    typeWriter.Write((short)0);
                    typeWriter.Write((short)4);
                    break;
            }
        }
        var nameBytes = new BlurEncoding().GetBytes(stringIndexer.ToString());
        
        typeStream.Position = 0;
        inheritenceStream.Position = 0;
        fieldStream.Position = 0;
        stream.Position = 0;

        serialization.DataTypesHeader = new Header(offset, (uint)(typeStream.Length / 12));
        typeStream.CopyTo(stream);
        
        offset += (uint)typeStream.Length;
        serialization.InheritenceHeader = new Header(offset, (uint)(inheritenceStream.Length / 4));
        inheritenceStream.CopyTo(stream);
        
        offset += (uint)inheritenceStream.Length;
        serialization.DataFieldsHeader = new Header(offset, (uint)(fieldStream.Length / 8));
        fieldStream.CopyTo(stream);

        offset += (uint)fieldStream.Length;
        serialization.TypeNamesHeader = new Header(offset, (uint)nameBytes.Length);
        stream.Write(nameBytes);

        offset += (uint)nameBytes.Length;
    }

    public class BlurSerialization
    {
        public Header DataTypesHeader { get; set; }
        public Header InheritenceHeader { get; set; }
        public Header DataFieldsHeader { get; set; }
        public Header TypeNamesHeader { get; set; }
        public Header RecordsHeader { get; set; }
        public Header EntitiesHeader { get; set; }
        public Header BlocksHeader { get; set; }
        public Header DataHeader { get; set; }

        public List<DataTypeDefinition> TypeDefinitions { get; } = new List<DataTypeDefinition>();
        public List<int> InheritenceDefinitions { get; } = new List<int>();
        public List<DataFieldDefinition> FieldDefinitions { get; } = new List<DataFieldDefinition>();

        public char[] TypeNames { get; set; } = new char[0];

        public List<RecordDefinition> RecordDefinitions { get; } = new();
        public List<EntityDefinition> EntityDefinitions { get; } = new();
        public List<BlockDefinition> BlockDefinitions { get; } = new();
    }
}