using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Types;
public enum FieldType
{
    Primitive = 0,
    Enum = 1,
    FlagsEnum = 2,
    Struct = 3,
    Pointer = 4,
    ExternalPointer = 5,
    EnumValue = 6,
    PrimitiveArray = 256,
    EnumArray = 257,
    StructArray = 259,
    PointerArray = 260,
    ExternalArray = 261,
}
