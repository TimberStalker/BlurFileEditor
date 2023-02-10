using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities;
public interface IEntityData
{
    public object Value { get; }
    public DataType DataType { get; }
    public DataField? DataField { get; set; }
}
