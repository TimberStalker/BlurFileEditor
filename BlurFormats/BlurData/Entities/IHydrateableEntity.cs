using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public interface IHydrateableEntity
{
    void Hydrate(RecordHeap heap, List<EntityBlock> block);
}
