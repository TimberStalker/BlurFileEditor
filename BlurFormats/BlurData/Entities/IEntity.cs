using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public interface IEntity
{
    public object Value { get; }
    public Guid Guid { get; }
    public DataType Type { get; }
}
