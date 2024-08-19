using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.FIlter;
public class EntityFilterNegate : IEntityFilter
{
    public IEntityFilter Filter { get; }
    public EntityFilterNegate(IEntityFilter filter)
    {
        Filter = filter;
    }


    public bool Matches(IEntity entity)
    {
        return !Filter.Matches(entity);
    }
}
