using BlurFormats.BlurData.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class ExternalArrayPointerEntity : ArrayPointerEntity, IExternalPointerEntity
{
    public ExternalArrayPointerEntity(DataType dataType, short pointer, short modifier, int length, FieldType fieldType) : base(dataType, pointer, modifier, length, fieldType)
    {
    }

    public override IEntity GetReplacement(PointerDataSource dataSource)
    {
        if (Pointer != -1 && dataSource.TryGetData(out List<BlurRecord>? records))
        {
            int offset = Pointer;
            var result = new ArrayEntity(Type);
            while(result.Value.Count < Length)
            {
                if (records[offset].Entity.Type != Type) continue;

                //result.Value.Add( new RecordEntity(Type, records[offset++]);
            }
            return result;
        }
        return new NullEntity(Type);
    }
}
