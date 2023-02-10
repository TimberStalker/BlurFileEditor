using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ArrayEntity : IEntity, IHydrateableEntity
{
    private readonly int type;
    private readonly int offset;
    private readonly int length;

    public ObservableCollection<ArrayEntityItem> Value { get; }

    object IEntity.Value => Value;

    public Guid Guid { get; }

    public DataType Type { get; }

    public ArrayEntity(DataType dataType)
    {
        Guid = Guid.NewGuid();
        Value = new ObservableCollection<ArrayEntityItem>();
        Type = dataType;
    }
    public ArrayEntity(DataType dataType, int type, int offset, int length)
    {
        Guid = Guid.NewGuid();
        Value = new ObservableCollection<ArrayEntityItem>();
        Type = dataType;
        this.type = type;
        this.offset = offset;
        this.length = length;
    }
    public void AddEntity(ReferenceEntity entity)
    {
        Value.Add(new ArrayEntityItem(this, entity));
    }
    public void Hydrate(RecordHeap heap, List<EntityBlock> blocks)
    {
        if (type == -1) return;
        var block = blocks[type];
        for(int i = offset; i < offset + length; i++) 
        {
            var entity = block.Value[i];
            if(entity is ForwardEntity f)
            {
                entity = f.GetEntity(blocks);
            }
            heap.AddEntity(entity);
            AddEntity(heap.GetReference(entity));
            
        }
    }
    public override string ToString() => $"{Type.Name}[] : {Value}";
}
