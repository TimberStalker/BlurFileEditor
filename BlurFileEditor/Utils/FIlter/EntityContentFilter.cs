using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.FIlter;
public class EntityContentFilter : IEntityFilter
{
    public string? Content { get; set; }
    public bool Matches(IEntity entity)
    {
        if(Content is null) return false;
        if(entity is not PrimitiveEntity) return false;
        return entity.Value.ToString()?.Contains(Content, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
