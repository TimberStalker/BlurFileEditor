using BlurFormats.Serialization;
using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Flask;
public class TypeDictionary
{
    List<SerializationType> types = [];
    public IReadOnlyList<SerializationType> Types => types;

    public bool TryRegister(SerializationType type)
    {
        if (types.Any(t => t.Name == type.Name))
        {
            return false;
        }
        types.Add(type);
        return true;
    }
    public bool TryGetType(string name, [NotNullWhen(true)] out SerializationType? type)
    {
        type = types.FirstOrDefault(type => type.Name == name);
        return type is not null;
    }
    public bool CanRename(SerializationType serializationType, string name)
    {
        return types.Contains(serializationType) && !types.Any(t => t.Name == name);
    }
    public bool TryRename(SerializationType serializationType, string name)
    {
        if (!CanRename(serializationType, name)) return false;
        serializationType.Name = name;
        return true;
    }
}
public class RecordDictionary
{
    List<SerializationRecord> records = [];
    public IReadOnlyList<SerializationRecord> Records => records;

    public bool TryAdd(SerializationRecord record)
    {
        if (records.Any(t => t.ID == record.ID))
        {
            return false;
        }
        records.Add(record);
        return true;
    }
    public bool TryRemove(SerializationRecord record)
    {
        if (!records.Any(t => t.ID == record.ID))
        {
            return false;
        }
        records.Remove(record);
        return true;
    }
    public bool TryGetType(uint id, [NotNullWhen(true)] out SerializationRecord? record)
    {
        record = records.FirstOrDefault(type => type.ID == id);
        return record is not null;
    }
}