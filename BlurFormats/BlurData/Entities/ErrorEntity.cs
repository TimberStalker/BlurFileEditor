using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ErrorEntity : IEntity
{
    public Exception Value { get; }

    object IEntity.Value => Value.Message;

    public Guid Guid { get; }

    public DataType Type { get; }

    public ErrorEntity(DataType dataType, Exception value)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Value = value;
    }
}
