using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class LocPointerEntity : PointerEntity, IPointerEntity
{
    public LocPointerEntity(DataType dataType, int value) : base(dataType, value)
    {
    }
    public override IEntity GetReplacement(PointerDataSource dataSource) => this;
}
