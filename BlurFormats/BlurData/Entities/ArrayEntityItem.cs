using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ArrayEntityItem
{
    public ArrayEntity Parent { get; }
    public ReferenceEntity Entity { get; }

    public int Index => Parent.Value.IndexOf(this);
    public ArrayEntityItem(ArrayEntity parent, ReferenceEntity entity)
    {
        Parent = parent;
        Entity = entity;
    }
}
