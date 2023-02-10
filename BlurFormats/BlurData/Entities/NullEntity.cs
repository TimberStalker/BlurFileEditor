using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class NullEntity : Entity
{
    public NullEntity(DataType dataType) : base(dataType, -1)
    {
    }
}
