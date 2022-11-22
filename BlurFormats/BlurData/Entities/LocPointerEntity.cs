using BlurFormats.BlurData.Entities.Pointers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class LocPointerEntity : PointerEntity
{
    public LocPointerEntity(DataType dataType, int value) : base(dataType, value)
    {
    }
    public override Entity GetReplacement(PointerDataSource dataSource) => this;
}
