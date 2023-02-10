using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ErrorEntity : Entity
{
    new public Exception Value => (Exception)base.Value;
    public ErrorEntity(DataType dataType, Exception value) : base(dataType, value)
    {
    }
}
