using BlurFormats.Serialization.Entities;
using BlurFormats.Serialization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.FIlter;
public class EntityTypeFilter : IEntityFilter
{
    public bool AllowSubclasses { get; set; } = true;
    public string? Type { get; set; }

    public bool Matches(IEntity entity)
    {
        if(Type is null) return false;
        if(entity.Type.Name == Type) return true;
        if (AllowSubclasses && entity.Type is StructureType s && s.IsSubclassOf(Type)) return true;
        return false;
    }
}
