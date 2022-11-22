using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class EnumEntity : Entity
{
    new public int Value
    {
        get => (int)base.Value; 
        set
        {
            base.Value = value;
            UpdateProperty(nameof(Value));
        }
    }
    public EnumEntity(DataType dataType, int value) : base(dataType, value)
    {
    }

    public override string ToString() => $"{DataType.Name} {DataField?.Name} = {Value}";
}
