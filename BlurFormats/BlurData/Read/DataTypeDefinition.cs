using System.Runtime.InteropServices;
using BlurFormats.Utils;

namespace BlurFormats.BlurData.Read;
public struct DataTypeDefinition : IReadable
{
    public ushort StringOffset { get; private set; }
    public ushort ItemCount { get; private set; }
    public ushort HasParent { get; private set; }
    public ushort StructureType { get; private set; }
    public ushort PrimitiveType { get; private set; }
    public ushort Size { get; private set; }

    public void Read(ref Reader reader)
    {
        StringOffset = reader.ReadUShort();
        ItemCount = reader.ReadUShort();
        HasParent = reader.ReadUShort();
        StructureType = reader.ReadUShort();
        PrimitiveType = reader.ReadUShort();
        Size = reader.ReadUShort();
    }
}