using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.FIlter;
public class SpecificEntityFilter : IEntityFilter
{
    public IEntity? Entity { get; set; }
    public bool Matches(IEntity entity) => entity == Entity;
}
