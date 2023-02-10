using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class EnumEntity : IEntity
{
    public int Value { get; set; }

    object IEntity.Value => Value;

    public Guid Guid { get; }

    public DataType Type { get; }

    public EnumEntity(DataType dataType, int value)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Value = value;
    }

    public override string ToString() => $"{Type.Name} : {Value}";
}
