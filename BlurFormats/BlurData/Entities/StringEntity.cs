using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class StringEntity : Entity
{
    new public string Value
    {
        get => (string)base.Value; 
        set => base.Value = value;
    }
    public StringEntity(DataType dataType, string value) : base(dataType, value)
    {
    }
}
