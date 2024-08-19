using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.FIlter;
public class MultiEntityFilter : IEntityFilter
{
    public FilterType Type { get; set; }
    public IEntityFilter Left { get; set; }
    public IEntityFilter Right { get; set; }
    public MultiEntityFilter(IEntityFilter left, IEntityFilter right)
    {
        Left = left;
        Right = right;
    }
    public bool Matches(IEntity entity) => 
        Type switch
        {
            FilterType.And => Left.Matches(entity) && Right.Matches(entity),
            FilterType.Or => Left.Matches(entity) || Right.Matches(entity),
            FilterType.Xor => Left.Matches(entity) ^ Right.Matches(entity),
            _ => false
        };
    public enum FilterType
    {
        And,
        Or,
        Xor
    }
}
