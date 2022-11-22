using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class EntityBlock
{
    public List<List<Entity>> Entities { get; } = new List<List<Entity>>();
    public byte[]? ExtraBytes { get; set; }
    public EntityBlock()
    {
    }
}
