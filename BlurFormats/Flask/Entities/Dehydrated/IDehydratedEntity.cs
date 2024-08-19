using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Serialization.Entities.Dehydrated;
internal interface IDehydratedEntity : IEntity
{
    int Priority { get; }
    public IEntity GetReplacement(Dictionary<string, object> pointerData);
}
