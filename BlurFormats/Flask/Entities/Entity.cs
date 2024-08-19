using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Serialization.Entities;
public static class Entity
{
    public static IEntity Clone(this IEntity entity)
    {
        throw new NotImplementedException();
        //Dictionary<IEntity, IEntity> mappings = new Dictionary<IEntity, IEntity>();
        //return entity.Clone(mappings);
    }
}
