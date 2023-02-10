using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities;
public sealed class EntityBlock
{
    public List<IEntityData> Data { get; } = new List<IEntityData>();
    public byte[]? ExtraBytes { get; set; } = Array.Empty<byte>();
}
