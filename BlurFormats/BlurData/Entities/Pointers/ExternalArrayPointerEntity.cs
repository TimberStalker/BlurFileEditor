using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class ExternalArrayPointerEntity : ArrayPointerEntity
{
    public ExternalArrayPointerEntity(DataType dataType, short pointer, short modifier, int length) : base(dataType, pointer, modifier, length)
    {
    }

    public override Entity GetReplacement(PointerDataSource dataSource)
    {
        return this;
    }
}
