using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class PrimitiveEntity<T> : Entity where T : notnull
{
    new public T Value 
    { 
        get => (T)base.Value; 
        set
        {
            base.Value = value;
            UpdateProperty(nameof(Value));
        }
    }
    public PrimitiveEntity(DataType dataType, T value) : base(dataType, value)
    {
    }
}
