using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData;
public enum DecodeType : ushort
{
    None,
    Bool,
    Byte,
    Short,
    Integer,
    Long,
    UByte,
    UShort,
    UInt,
    ULong,
    Float,
    Double,
    String,
    Localaization = 14
}
