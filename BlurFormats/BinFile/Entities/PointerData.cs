using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities;
public struct PointerData : IPointerData
{
    public uint Pointer { get; set; }
	public PointerData(uint pointer)
	{
		Pointer = pointer;
	}
	public override string ToString() => $"P:{Pointer}";
}
