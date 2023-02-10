using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities
{
    [DebuggerDisplay("{DataType?.Name ?? \"None\"} {DataField?.Name ?? \"\"} = {ToString()}")]
    public class EntityData<T> : IEntityData where T : notnull
    {
        public DataType DataType { get; set; }
        public DataField? DataField { get; set; }
        public T Value { get; set; }

        object IEntityData.Value => Value;

        public EntityData(DataType dataType)
        {
            DataType = dataType;
        }

        public override string ToString()
        {
            if (Value is EnumData e)
            {
                bool flags = DataType.ReadType == 2;
                if (flags)
                {
                    return string.Join(" | ", DataType.Fields.Where((f, i) => ((1 << i) & e.BaseValue) > 0).Select(f => f.Name));
                }
                else
                {
                    return DataType.Fields[(int)e.BaseValue].Name;
                }
            }
            else
            {
                return Value.ToString()!;
            }
        }
    }
}
