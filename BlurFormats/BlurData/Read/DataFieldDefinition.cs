using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Read;
public struct DatafieldDefinition : IReadable
{
    public ushort StringOffset { get; private set; }
    public ushort StructureType { get; private set; }
    public ushort Offset { get; private set; }
    public ushort ReadType { get; private set; }

    public void Read(ref Reader reader)
    {
        StringOffset = reader.ReadUShort();
        StructureType = reader.ReadUShort();
        Offset = reader.ReadUShort();
        ReadType = reader.ReadUShort();
    }
}
