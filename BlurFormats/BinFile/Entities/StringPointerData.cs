using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities;
public struct StringPointerData : IPointerData
{
    public uint Pointer { get; set; }
	public StringPointerData(uint pointer)
	{
		Pointer = pointer;
	}
	public override string ToString() => $"S:{Pointer}";
}
