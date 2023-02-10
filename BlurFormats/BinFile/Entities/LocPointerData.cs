using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities;
public struct LocPointerData : IPointerData
{
    public uint Pointer { get; set; }
	public LocPointerData(uint pointer)
	{
		Pointer = pointer;
	}

	public override string ToString() => $"L:{Pointer}";
}
