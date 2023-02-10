using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class EntityBlock : IEntity
{
    public List<IEntity> Value { get; }

    object IEntity.Value => Value;
    public Guid Guid { get; }

    public DataType Type { get; set; }

    public EntityBlock(DataType dataType)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Value = new List<IEntity>();
    }
}
