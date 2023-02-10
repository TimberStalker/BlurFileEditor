using System.Runtime.InteropServices;
using BlurFormats.Utils;

namespace BlurFormats.BinFile.Definitions;
public struct DataTypeDefinition : IReadable
{
    public ushort StringOffset { get; private set; }
    public ushort ItemCount { get; private set; }
    public ushort Unknown { get; private set; }
    public ushort ReadType { get; private set; }
    public ushort DecodeType { get; private set; }
    public ushort Size { get; private set; }

    public void Read(ref Reader reader)
    {
        StringOffset = reader.ReadUShort();
        ItemCount = reader.ReadUShort();
        Unknown = reader.ReadUShort();
        ReadType = reader.ReadUShort();
        DecodeType = reader.ReadUShort();
        Size = reader.ReadUShort();
    }
}