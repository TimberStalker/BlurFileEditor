using BlurFormats.BlurData.Entities.Pointers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class PointerEntity : Entity, IPointerEntity
{
    new public int Value
    {
        get => (int)base.Value; 
        set => base.Value = value;
    }
    public PointerEntity(DataType dataType, int value) : base(dataType, value)
    {
    }
    public override string ToString() => $"{DataType.Name}* {DataField?.Name} = {Value}";

    public virtual Entity GetReplacement(PointerDataSource dataSource)
    {
        try
        {
            if(dataSource.TryGetData<EntityBlock>(out var entityBlock)) 
            { 
                if (Value == -1 || entityBlock.Entities.Count < Value-1) return new NullEntity(DataType) { DataField = DataField };
                return entityBlock.Entities[Value][0];
            }
            return new NullEntity(DataType)
            {
                DataField = DataField,

            };
        } catch(Exception e)
        {
            return new ErrorEntity(DataType, e) { DataField = DataField };
        }
    }
}
