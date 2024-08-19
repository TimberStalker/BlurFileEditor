using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;

[StructLayout(LayoutKind.Sequential)]
public struct Header
{
    public uint Start { get; private set; }
    public uint Length { get; private set; }
    public Header(uint start, uint length)
    {
        Start = start;
        Length = length;
    }
    public uint GetEnd(uint size) => Start + Length * size;
    public void Write(BinaryWriter writer)
    {
        writer.Write(Start);
        writer.Write(Length);
    }
    public static Header Read(BinaryReader reader)
    {
        uint start = reader.ReadUInt32();
        uint length = reader.ReadUInt32();
        return new Header(start, length);
    }
}
