using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Read;
[DebuggerDisplay("Entity:{Entity} L:{Length} S:{Size} O:{Offset}")]
public struct EntityNameDefinition : IReadable
{
    public int Entity { get; private set; }
    public int Length { get; private set; }
    public int Size { get; private set; }
    public int Offset { get; private set; }

    public void Read(ref Reader reader)
    {
        Entity = reader.ReadInt();
        Length = reader.ReadInt();
        Size = reader.ReadInt();
        Offset = reader.ReadInt();
    }
}
