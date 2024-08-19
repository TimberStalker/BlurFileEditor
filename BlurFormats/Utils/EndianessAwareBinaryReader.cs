using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public class EndiannessAwareBinaryReader : BinaryReader
{
    public Endianness Endianness { get; set; } = Endianness.Little;
    public EndiannessAwareBinaryReader(Stream input) : base(input)
    {
    }

    public EndiannessAwareBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
    {
    }

    public EndiannessAwareBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
    {
    }
    public override short ReadInt16() => ReadWithEndianness(BinaryPrimitives.ReadInt16LittleEndian, BinaryPrimitives.ReadInt16BigEndian, ReadBytes(2));

    public override ushort ReadUInt16() => ReadWithEndianness(BinaryPrimitives.ReadUInt16LittleEndian, BinaryPrimitives.ReadUInt16BigEndian, ReadBytes(2));

    public override int ReadInt32() => ReadWithEndianness(BinaryPrimitives.ReadInt32LittleEndian, BinaryPrimitives.ReadInt32BigEndian, ReadBytes(4));
    public override uint ReadUInt32() => ReadWithEndianness(BinaryPrimitives.ReadUInt32LittleEndian, BinaryPrimitives.ReadUInt32BigEndian, ReadBytes(4));
    public override long ReadInt64() => ReadWithEndianness(BinaryPrimitives.ReadInt64LittleEndian, BinaryPrimitives.ReadInt64BigEndian, ReadBytes(8));
    public override ulong ReadUInt64() => ReadWithEndianness(BinaryPrimitives.ReadUInt64LittleEndian, BinaryPrimitives.ReadUInt64BigEndian, ReadBytes(8));
    public override Half ReadHalf() => BitConverter.Int16BitsToHalf(ReadWithEndianness(BinaryPrimitives.ReadInt16LittleEndian, BinaryPrimitives.ReadInt16BigEndian, ReadBytes(2)));
    public override unsafe float ReadSingle() => BitConverter.Int32BitsToSingle(ReadWithEndianness(BinaryPrimitives.ReadInt32LittleEndian, BinaryPrimitives.ReadInt32BigEndian, ReadBytes(4)));
    public override unsafe double ReadDouble() => BitConverter.Int64BitsToDouble(ReadWithEndianness(BinaryPrimitives.ReadInt64LittleEndian, BinaryPrimitives.ReadInt64BigEndian, ReadBytes(8)));

    protected delegate T ReadWithEndiannessDelegate<T>(ReadOnlySpan<byte> bytes);

    protected T ReadWithEndianness<T>(ReadWithEndiannessDelegate<T> little, ReadWithEndiannessDelegate<T> big, byte[] bytes)
    {
        return Endianness switch
        {
            Endianness.Little => little(bytes),
            Endianness.Big => big(bytes),
            _ => throw new UnreachableException()
        };
    }
}
public enum Endianness
{
    Little,
    Big,
}