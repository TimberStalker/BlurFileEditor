using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Read;
[DebuggerDisplay("Type:{BaseType} Specifics:{EntitySpecifics} - {GlobalID}")]
public struct EntityDefinition : IReadable
{
    public uint GlobalID { get; private set; }
    public ushort BaseType { get; private set; }
    public short EntitySpecifics { get; private set; }

    public string GlobalIDHex => GlobalID.ToString("X4");
    public void Read(ref Reader reader)
    {
        GlobalID = reader.ReadUInt();
        BaseType = reader.ReadUShort();
        EntitySpecifics = reader.ReadShort();
    }
}
