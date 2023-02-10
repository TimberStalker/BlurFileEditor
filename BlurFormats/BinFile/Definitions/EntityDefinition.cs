using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BinFile.Definitions;
[DebuggerDisplay("Type:{EntityType} Name:{EntityName} - {GlobalID}")]
public struct EntityDefinition : IReadable
{
    public uint GlobalID { get; private set; }
    public ushort EntityType { get; private set; }
    public ushort EntityName { get; private set; }

    public string GlobalIDHex => GlobalID.ToString("X4");
    public void Read(ref Reader reader)
    {
        GlobalID = reader.ReadUInt();
        EntityType = reader.ReadUShort();
        EntityName = reader.ReadUShort();
    }
}
