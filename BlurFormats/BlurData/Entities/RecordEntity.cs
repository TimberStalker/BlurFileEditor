using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class RecordEntity : IEntity
{
    public BlurRecord Record { get; }

    object IEntity.Value => Record;

    public Guid Guid { get; }

    public DataType Type { get; }

    public RecordEntity(DataType dataType, BlurRecord record)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Record = record;
    }
}
