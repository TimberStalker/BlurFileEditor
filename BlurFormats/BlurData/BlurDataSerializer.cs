using BlurFormats.BlurData.Entities;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData;
internal ref struct BlurDataSerializer
{
    int offset = 72;
    BlurData BlurData { get; }
    Stream Stream { get; }
    BinaryWriter FinalWriter { get; }

    public BlurDataSerializer(BlurData blurData, Stream stream)
    {
        BlurData = blurData;
        Stream = stream;
        FinalWriter = new BinaryWriter(stream);

        FinalWriter.Write(Encoding.ASCII.GetBytes("KSLF"));
        FinalWriter.Write(2);
    }


    public void Serialize()
    {
        var typesStream = new MemoryStream();
        SerializeTypes(typesStream);

        using var dataStream = new MemoryStream();
        SerializeRecords(dataStream);

        typesStream.Position = 0;
        dataStream.Position = 0;

        typesStream.CopyTo(Stream);
        dataStream.CopyTo(Stream);
    }

    private void SerializeTypes(MemoryStream fullTypesStream)
    {
        Dictionary<string, int> stringOffsets = new Dictionary<string, int>();
        StringBuilder stringsText = new StringBuilder();

        using var typeStream = new MemoryStream();
        using var inheritenceStream = new MemoryStream();
        using var fieldStream = new MemoryStream();

        using var typeWriter = new BinaryWriter(typeStream);
        using var inheritenceWriter = new BinaryWriter(inheritenceStream);
        using var fieldWriter = new BinaryWriter(fieldStream);

        for (int i = 0; i < BlurData.DataTypes.Count; i++)
        {
            var type = BlurData.DataTypes[i];

            if (!stringOffsets.ContainsKey(type.Name))
            {
                stringOffsets.Add(type.Name, stringsText.Length);
                stringsText.Append(type.Name);
                stringsText.Append('\0');
            }
            typeWriter.Write((short)stringOffsets[type.Name]);
            typeWriter.Write((short)type.Fields.Count);
            for (int j = 0; j < type.Fields.Count; j++)
            {
                var field = type.Fields[j];

                if (!stringOffsets.ContainsKey(field.Name))
                {
                    stringOffsets.Add(field.Name, stringsText.Length);
                    stringsText.Append(field.Name);
                    stringsText.Append('\0');
                }
                fieldWriter.Write((short)stringOffsets[field.Name]);
                fieldWriter.Write((short)(field.DataType is not null ? BlurData.DataTypes.IndexOf(field.DataType) : -1));
                fieldWriter.Write((short)type.GetOffset(field));
                fieldWriter.Write((short)field.FieldType);
            }
            if (type.Base is not null)
            {
                typeWriter.Write((short)1);
                inheritenceWriter.Write(BlurData.DataTypes.IndexOf(type.Base));
            }
            else
            {
                typeWriter.Write((short)0);
            }
            typeWriter.Write((short)type.StructureType);
            typeWriter.Write((short)type.PrimitiveType);
            typeWriter.Write((short)type.Size);
        }
        var nameBytes = Reader.Encrypt(Encoding.ASCII.GetBytes(stringsText.ToString()));

        typeStream.Position = 0;
        inheritenceStream.Position = 0;
        fieldStream.Position = 0;
        fullTypesStream.Position = 0;

        typeStream.CopyTo(fullTypesStream);
        inheritenceStream.CopyTo(fullTypesStream);
        fieldStream.CopyTo(fullTypesStream);
        fullTypesStream.Write(nameBytes);

        WriteHeader(BlurData.DataTypes.Count, (int)typeStream.Length);
        WriteHeader((int)inheritenceStream.Length / 4, (int)inheritenceStream.Length);
        WriteHeader((int)fieldStream.Length / 8, (int)fieldStream.Length);
        WriteHeader(nameBytes.Length, nameBytes.Length);
    }

    private void SerializeRecords(MemoryStream fullDataStream)
    {
        var recordsStream = new MemoryStream();
        var entitiesStream = new MemoryStream();
        var blocksStream = new MemoryStream();
        var dataStream = new MemoryStream();

        var recordsWriter = new BinaryWriter(recordsStream);
        var entitiesWriter = new BinaryWriter(entitiesStream);
        var blocksWriter = new BinaryWriter(blocksStream);
        var dataWriter = new BinaryWriter(dataStream);

        int entityIndex = 0;
        for (int i = 0; i < BlurData.Records.Count; i++)
        {
            BlurRecord? record = BlurData.Records[i];
            recordsWriter.Write(record.ID);
            recordsWriter.Write((short)BlurData.DataTypes.IndexOf(record.BaseType));
            if (record.Entity is not NullEntity)
            {
                recordsWriter.Write(entityIndex++);
                entitiesWriter.Write(i);

                WriteEntityBlocks(record.Entity, dataWriter, blocksWriter, out int blockCount, out string names);
                entitiesWriter.Write(names.Length);
                dataWriter.Write(new BlurEncoding().GetBytes(names));
            }
            else
            {
                recordsWriter.Write(-1);
            }
        }

        WriteHeader(BlurData.Records.Count, (int)recordsStream.Length);
        WriteHeader(entityIndex, (int)entitiesStream.Length);
        WriteHeader(0, (int)blocksStream.Length);
        WriteHeader(0, (int)dataStream.Length);
    }
    void WriteEntityBlocks(IEntity entity, BinaryWriter dataWriter, BinaryWriter blocksWriter, out int length, out string names)
    {
        List<(DataType, List<IEntity>)> entityBlocks = new List<(DataType, List<IEntity>)>
        {
            (entity.Type, new List<IEntity> { entity })
        };
        Dictionary<string, int> addedStringIndecies = new Dictionary<string, int>();
        StringBuilder strings = new StringBuilder();


        length = 0;
        names = strings.ToString();
    }
    public void WriteHeader(int count, int length)
    {
        FinalWriter.Write(offset);
        FinalWriter.Write(count);
        offset += length;
    }
}
