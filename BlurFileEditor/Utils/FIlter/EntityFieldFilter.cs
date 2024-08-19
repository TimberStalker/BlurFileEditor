using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.FIlter;
public class EntityFieldFilter : IEntityFilter
{
    public IEntityFilter Inner { get; }
    public string Name { get; }

    public EntityFieldFilter(IEntityFilter inner, string name)
    {
        Inner = inner;
        Name = name;
    }

    public bool Matches(IEntity entity)
    {
        var field = entity.GetField(Name);
        if (field is null) return false;
        return Inner.Matches(field);
    }
}
