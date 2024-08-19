using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public interface IExternalHydrateableEntity
{
    void Hydrate(IReadOnlyList<BlurRecord> records, List<EntityBlock> block);
}
