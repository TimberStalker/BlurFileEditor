using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class ExternalPointerEntity : PointerEntity, IExternalPointerEntity
{
    public ExternalPointerEntity(DataType dataType, int value) : base(dataType, value)
    {
    }
    public override IEntity GetReplacement(PointerDataSource dataSource)
    {
        if (Pointer != -1 && dataSource.TryGetData(out List<BlurRecord>? records))
        {
            return new RecordEntity(Type, records[Pointer]);
        }
        return new NullEntity(Type);
    }
}
