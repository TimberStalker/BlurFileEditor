using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ReferenceEntity : IEntity, IHydrateableEntity
{
    RecordHeap? heap;
    int BlockType { get; }
    int Offset { get; }
    public IEntity? Reference { get; set; }

    public DataType Type { get; }
    public Guid Guid { get; }
    public IEntity Entity => Reference ?? new NullEntity(Type);
    public object Value => Entity.Value;

    public ReferenceEntity(DataType type, RecordHeap heap)
    {
        Type = type;
        Guid = Guid.NewGuid();
        this.heap = heap;
    }
    public ReferenceEntity(DataType type, int blockType, int offset)
    {
        Type = type;
        Guid = Guid.NewGuid();
        BlockType = blockType;
        Offset = offset;
    }

    public void Hydrate(RecordHeap heap, List<EntityBlock> blocks) 
    {
        this.heap = heap;

        if (Offset < 0)
        {
            return;
        }
        
        var block = blocks[BlockType];
        var entity = block.Value[Offset];
        if(entity is ForwardEntity f)
        {
            entity = f.GetEntity(blocks);
        }
        heap.AddEntity(entity);
        Reference = entity;
    }
}
