using BlurFormats.Serialization.Entities;
using BlurFormats.Serialization.Entities.Dehydrated;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BlurFormats.Serialization.Types;
[DebuggerDisplay("{Name} ({Primitive})")]
public class PrimitiveType : SerializationType
{
    public override int Size => 4;
    public Primitives Primitive { get; init; }
    public Type InternalType => GetType(Primitive);
    public IEntity DeserializeValue(BinaryReader reader) => Primitive switch
    {
        //Primitives.None => new NullEntity(dataType),
        Primitives.Bool => new PrimitiveEntity(this, reader.ReadInt32() > 0),
        Primitives.Byte => new PrimitiveEntity(this, (sbyte)reader.ReadInt32()),
        Primitives.Short => new PrimitiveEntity(this, (short)reader.ReadInt32()),
        Primitives.Integer => new PrimitiveEntity(this, reader.ReadInt32()),
        Primitives.Long => new PrimitiveEntity(this, reader.ReadInt64()),
        Primitives.UByte => new PrimitiveEntity(this, (byte)reader.ReadInt32()),
        Primitives.UShort => new PrimitiveEntity(this, (ushort)reader.ReadInt32()),
        Primitives.UInt => new PrimitiveEntity(this, reader.ReadUInt32()),
        Primitives.ULong => new PrimitiveEntity(this, reader.ReadUInt64()),
        Primitives.Float => new PrimitiveEntity(this, reader.ReadSingle()),
        Primitives.Double => new PrimitiveEntity(this, reader.ReadDouble()),
        Primitives.String => new ReferenceEntity(this, new DehydratedStringEntity(this, reader.ReadInt32())),
        Primitives.Localization => new ExternalLocEntity(this, reader.ReadInt32()),
        var i => throw new Exception($"{i} is not yet a handled decode type.")
    };
    public void SerializeValue(BinaryWriter writer, IEntity entity)
    {
        switch (Primitive)
        {
            case Primitives.None:
                break;
            case Primitives.Bool:
                writer.Write((bool)entity.Value == true ? 1 : 0);
                break;
            case Primitives.Byte:
                writer.Write((int)(sbyte)entity.Value);
                break;
            case Primitives.Short:
                writer.Write((int)(short)entity.Value);
                break;
            case Primitives.Integer:
                writer.Write((int)entity.Value);
                break;
            case Primitives.Long:
                writer.Write((long)entity.Value);
                break;
            case Primitives.UByte:
                writer.Write((uint)(byte)entity.Value);
                break;
            case Primitives.UShort:
                writer.Write((uint)(ushort)entity.Value);
                break;
            case Primitives.UInt:
                writer.Write((uint)entity.Value);
                break;
            case Primitives.ULong:
                writer.Write((ulong)entity.Value);
                break;
            case Primitives.Float:
                writer.Write((float)entity.Value);
                break;
            case Primitives.Double:
                writer.Write((double)entity.Value);
                break;
            case Primitives.String:
                writer.Write((int)entity.Value);
                break;
            case Primitives.Localization:
                writer.Write((int)entity.Value);
                break;
            default:
                throw new Exception($"{Primitive} is not yet a handled decode type.");
        }
    }

    public enum Primitives : short
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
        Localization = 14
    }
    public static Primitives GetPrimitive(Type t) => t switch
    {
        Type p when p == typeof(bool) => Primitives.Bool,
        Type p when p == typeof(sbyte) => Primitives.Byte,
        Type p when p == typeof(short) => Primitives.Short,
        Type p when p == typeof(int) => Primitives.Integer,
        Type p when p == typeof(long) => Primitives.Long,
        Type p when p == typeof(byte) => Primitives.UByte,
        Type p when p == typeof(ushort) => Primitives.UShort,
        Type p when p == typeof(uint) => Primitives.UInt,
        Type p when p == typeof(ulong) => Primitives.ULong,
        Type p when p == typeof(float) => Primitives.Float,
        Type p when p == typeof(double) => Primitives.Double,
        Type p when p == typeof(string) => Primitives.String,
        _ => Primitives.None,
    };
    public static Type GetType(Primitives p) => p switch
    {
        Primitives.Bool  => typeof(bool),
        Primitives.Byte => typeof(sbyte),
        Primitives.Short => typeof(short),
        Primitives.Integer => typeof(int),
        Primitives.Long => typeof(long),
        Primitives.UByte => typeof(byte),
        Primitives.UShort => typeof(ushort),
        Primitives.UInt => typeof(uint),
        Primitives.ULong => typeof(ulong),
        Primitives.Float => typeof(float),
        Primitives.Double => typeof(double),
        Primitives.String => typeof(string),
        _ => typeof(object),
    };

    public override void WriteXml(XmlWriter writer)
    {
        writer.WriteStartElement(Name);
        writer.WriteEndElement();
    }
    public override string ToString() => $"Primitive<{Primitive}>";
}
