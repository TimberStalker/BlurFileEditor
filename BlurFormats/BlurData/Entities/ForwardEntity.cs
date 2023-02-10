using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ForwardEntity : IEntity
{
    public int blockType, offset;
    object IEntity.Value => (blockType, offset);

    public Guid Guid { get; }

    public DataType Type { get; }
    public ForwardEntity(DataType type, int blockType, int offset)
    {
        Guid = Guid.NewGuid();
        Type = type;
        this.blockType = blockType;
        this.offset = offset;
    }

    public IEntity GetEntity(List<EntityBlock> blocks)
    {
        var block = blocks[blockType];
        var entity = block.Value[offset];
        if(entity is ForwardEntity f) 
        {
            entity = f.GetEntity(blocks);
        }
        return entity;
    }
}
