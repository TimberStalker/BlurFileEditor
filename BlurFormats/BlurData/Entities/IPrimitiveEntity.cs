using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public interface IPrimitiveEntity<T> : IEntity where T : notnull
{
    new public T Value { get; }
}
