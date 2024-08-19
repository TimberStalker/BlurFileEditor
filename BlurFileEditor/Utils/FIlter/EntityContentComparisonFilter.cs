using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.FIlter;
public class EntityContentComparisonFilter : IEntityFilter
{
    public EntityContentComparisonFilter(Func<PrimitiveEntity, bool> comparison)
    {
        Comparison = comparison;
    }

    public Func<PrimitiveEntity, bool> Comparison { get; }

    public bool Matches(IEntity entity)
    {
        if (entity is PrimitiveEntity p) return Comparison(p);
        else if (entity is ReferenceEntity r && r.Reference is PrimitiveEntity sp) return Comparison(sp);
        return false;
    }
}