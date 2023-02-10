using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities;
public struct DoublePointerData : IPointerData
{
    public ushort Pointer1 { get; set; }
    public ushort Pointer2 { get; set; }
    public uint Pointer => ((uint)Pointer1 << 16) + Pointer2;
    public DoublePointerData(ushort pointer1, ushort pointer2)
    {
        Pointer1 = pointer1;
        Pointer2 = pointer2;
    }
    public override string ToString() => $"{Pointer1} {Pointer2}";
}
