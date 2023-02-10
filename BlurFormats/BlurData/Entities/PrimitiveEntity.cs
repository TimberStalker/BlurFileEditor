using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class PrimitiveEntity<T> : IPrimitiveEntity<T> where T : notnull
{

    public T Value { get; set; }
    object IEntity.Value => Value;
    public Guid Guid { get; }
    public DataType Type { get; }

    public PrimitiveEntity(DataType type, T value)
    {
        Guid = Guid.NewGuid();
        Value = value;
        Type = type;
    }

    public override string ToString()
    {
        return Value.ToString()!;
    }
}
