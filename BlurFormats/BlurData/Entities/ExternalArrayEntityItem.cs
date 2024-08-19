using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ExternalArrayEntityItem : IArrayEntityItem
{
    public ExternalArrayEntity Parent { get; }
    public ExternalReferenceEntity Entity { get; }

    public int Index => Parent.Value.IndexOf(this);
    public ExternalArrayEntityItem(ExternalArrayEntity parent, ExternalReferenceEntity entity)
    {
        Parent = parent;
        Entity = entity;
    }
}
