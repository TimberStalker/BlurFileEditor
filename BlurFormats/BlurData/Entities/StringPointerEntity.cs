using BlurFormats.BlurData.Entities.Pointers;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class StringPointerEntity : PointerEntity
{
    public StringPointerEntity(DataType dataType, int value) : base(dataType, value)
    {
    }

    public override Entity GetReplacement(PointerDataSource dataSource)
    {
        if(dataSource.TryGetData<string>(out var strings))
        {
            return new StringEntity(DataType, strings.GetTerminatedStringAtOffset(Value))
            {
                DataField = DataField
            };
        }
        return new NullEntity(DataType)
        {
            DataField = DataField
        };
    }
}
