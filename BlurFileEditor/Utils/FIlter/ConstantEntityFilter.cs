using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.FIlter
{
    public class ConstantEntityFilter : IEntityFilter
    {
        public bool Value { get; }
        public ConstantEntityFilter(bool value)
        {
            Value = value;
        }

        public bool Matches(IEntity entity)
        {
            return Value;
        }
    }
}
