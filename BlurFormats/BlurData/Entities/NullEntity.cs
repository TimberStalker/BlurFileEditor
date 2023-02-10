using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class NullEntity : IEntity
{
    object IEntity.Value => -1;
    public DataType Type { get; }
    public Guid Guid { get; }


    public NullEntity(DataType type)
    {
        Guid = Guid.NewGuid();
        Type = type;
    }

}
