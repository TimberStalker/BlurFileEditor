using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public struct Header : IReadable
{
    public int Start { get; private set; }
    public int Length { get; private set; }

    public void Read(ref Reader reader)
    {
        Start = reader.ReadInt();
        Length = reader.ReadInt();
    }
}
