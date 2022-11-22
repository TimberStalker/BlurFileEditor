using BlurFormats.BlurData.Entities.Pointers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class DoublePointerEntity : Entity, IPointerEntity
{
    new (ushort pointer1, ushort pointer2) Value
    {
        get => ((ushort, ushort))base.Value;
        set
        {
            base.Value = value;
            UpdateProperty(nameof(Value));
        }
    }
    public ushort Pointer1
    {
        get => Value.pointer1;
        set
        {
            Value = Value with { pointer1 = value };
            UpdateProperty(nameof(Pointer1));
        }
    }
    public ushort Pointer2
    {
        get => Value.pointer2;
        set
        {
            Value = Value with { pointer2 = value };
            UpdateProperty(nameof(Pointer2));
        }
    }
    public DoublePointerEntity(DataType dataType, (ushort pointer1, ushort pointer2) value) : base(dataType, value)
    {
    }
    public override string ToString() => $"{DataType.Name}* {DataField?.Name} = {Value}";

    public Entity GetReplacement(PointerDataSource dataSource)
    {
        if(dataSource.TryGetData<EntityBlock>(out var entityBlock)) 
        { 
            return entityBlock.Entities[Pointer1][Pointer2];
        }
        return new NullEntity(DataType) { DataField = DataField };
    }
}
