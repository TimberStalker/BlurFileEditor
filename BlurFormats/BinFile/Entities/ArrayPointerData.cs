using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities;
public struct ArrayPointerData : IPointerData
{
    public uint Length { get; set; }
    public uint Pointer { get; set; }
    public ArrayPointerData(uint length, uint pointer)
    {
        Length = length;
        Pointer = pointer;
    }

    public override string ToString() => $"{Length} - {Pointer}";
}
