using BlurFormats.BlurData.Entities.Pointers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ArrayPointerEntity : Entity, IPointerEntity
{
    new (short pointer, short modifier, int length) Value
    {
        get => ((short, short, int))base.Value;
        set
        {
            base.Value = value;
            UpdateProperty(nameof(Value));
        }
    }
    public int Length
    {
        get => Value.length;
        set 
        {
            Value = Value with { length = value };
            UpdateProperty(nameof(Length));
        }
    }
    public short Pointer
    {
        get => Value.pointer;
        set
        {
            Value = Value with { pointer = value };
            UpdateProperty(nameof(Pointer));
        }
    }
    public short Modifier
    {
        get => Value.modifier;
        set
        {
            Value = Value with { modifier = value };
            UpdateProperty(nameof(Modifier));
        }
    }
    public ArrayPointerEntity(DataType dataType, short pointer, short modifier, int length) : base(dataType, (pointer, modifier, length))
    {

    }
    public override string ToString() => $"{DataType.Name}[{Length}] {DataField?.Name} = {Pointer}";

    public virtual Entity GetReplacement(PointerDataSource dataSource)
    {
        if(dataSource.TryGetData<EntityBlock>(out var entityBlock))
        {
            var replacement = new ArrayEntity(DataType)
            {
                DataField = DataField,
            };
            //try
            //{
                for(int i = 0; i < Length; i++)
                {
                    replacement.Value.Add(entityBlock.Entities[Pointer][i]);
                }
            //}
            //catch (Exception e)
            //{
            //    return new ErrorEntity(DataType, e) { DataField = DataField };
            //}
            return replacement;
        }
        return new NullEntity(DataType) { DataField = DataField };
    }
}
