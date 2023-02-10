using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Entities;
public struct EnumData
{
    public uint BaseValue { get; set; }
	public EnumData(uint baseValue)
	{
        BaseValue = baseValue;

    }
    public override string ToString() => $"E:{BaseValue}";
}
