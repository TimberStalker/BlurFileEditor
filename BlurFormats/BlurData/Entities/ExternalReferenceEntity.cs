using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ExternalReferenceEntity : IEntity
{
    RecordHeap? heap;
    int Offset { get; }
    public BlurRecord? Record { get; set; }

    public DataType Type { get; }
    public Guid Guid { get; }
    public IEntity Entity => Record?.Entity ?? new NullEntity(Type);
    public object Value => Entity.Value;

    public ExternalReferenceEntity(DataType type, RecordHeap heap)
    {
        Type = type;
        Guid = Guid.NewGuid();
        this.heap = heap;
    }
    public ExternalReferenceEntity(DataType type, int offset)
    {
        Type = type;
        Guid = Guid.NewGuid();
        Offset = offset;
    }

    public void Hydrate(List<BlurRecord> records)
    {
        if (Offset < 0) return;
        
        Record = records[Offset];
    }
}
