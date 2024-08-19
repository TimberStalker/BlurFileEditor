using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ExternalForwardEntity : IEntity
{
    public int offset;
    object IEntity.Value => offset;

    public Guid Guid { get; }

    public DataType Type { get; }
    public ExternalForwardEntity(DataType type, int offset)
    {
        Guid = Guid.NewGuid();
        Type = type;
        this.offset = offset;
    }

    public BlurRecord GetRecord(IReadOnlyList<BlurRecord> blocks)
    {
        return blocks[offset];
    }
}
