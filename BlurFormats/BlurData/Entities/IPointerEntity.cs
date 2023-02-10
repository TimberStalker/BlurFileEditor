using BlurFormats.BlurData.Entities.Pointers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public interface IPointerEntity
{
    public Entity GetReplacement(PointerDataSource dataSource);
}
