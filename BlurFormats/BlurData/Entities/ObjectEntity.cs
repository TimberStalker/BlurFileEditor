using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ObjectEntity : IEntity
{
    public ObjectEntityItem? this[string name] => GetValue(name);
    public List<ObjectEntityItem> Fields { get; }

    public object Value => Fields;

    public Guid Guid { get; }

    public DataType Type { get; set; }

    public ObjectEntity(DataType dataType)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Fields = new List<ObjectEntityItem>();
    }
    public ObjectEntityItem? GetValue(string name) => Fields.FirstOrDefault(i => i.Field?.Name == name);
}
[DebuggerDisplay("{Field.Name} {Field.DataType.Name} = {Value}")]
public class ObjectEntityItem
{
    public DataField? Field { get; set; }
    public IEntity Value { get; set; }
    public ObjectEntityItem(DataField? field, IEntity value)
    {
        Field = field;
        Value = value;
    }
}