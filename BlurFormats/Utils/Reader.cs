using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public ref struct Reader
{
    public int Position { get; private set; }
    ReadOnlySpan<byte> Bytes { get; }
    public int Length => Bytes.Length;
    public byte Peek => Bytes[Position];
    public Reader(ReadOnlySpan<byte> bytes)
    {
        Position = 0;
        Bytes = bytes;
    }
    public Reader Subreader(int length) => new Reader(Read(length));
    public Reader Subreader(int start, int length)
    {
        Seek(start);
        return Subreader(length);
    }
    public void Seek(int position) => Position = position;
    public void Offset(int offset) => Position += offset;
    public ReadOnlySpan<byte> Read(int count = 4) => Bytes.Slice((Position += count) - count, count);

    public byte ReadByte() => Read(1)[0];
    public sbyte ReadSByte() => (sbyte)Read(1)[0];
    public short ReadShort() => BitConverter.ToInt16(Read(2));
    public ushort ReadUShort() => BitConverter.ToUInt16(Read(2));
    public int ReadInt() => BitConverter.ToInt32(Read());
    public uint ReadUInt() => BitConverter.ToUInt32(Read());
    public long ReadLong() => BitConverter.ToInt64(Read(8));
    public ulong ReadULong() => BitConverter.ToUInt64(Read(8));
    public float ReadFloat() => BitConverter.ToSingle(Read());
    public double ReadDouble() => BitConverter.ToDouble(Read(8));
    public char ReadChar() => BitConverter.ToChar(Read(4));
    public string ReadString() => Encoding.ASCII.GetString(Read(ReadInt()));
    public string ReadString(int length) => Encoding.ASCII.GetString(Read(length));
    public string ReadStringTerminated()
    {
        int start = Position;
        int count = 0;

        do
        {
            count += 1;
        } while (ReadByte() != 0);

        return Encoding.ASCII.GetString(Bytes.Slice(start, count - 1));
    }
    public string ReadString(Encoding encoding) => encoding.GetString(Read(ReadInt()));
    public string ReadString(int length, Encoding encoding) => encoding.GetString(Read(length));
    public string ReadUnicodeStringTerminated()
    {
        int start = Position;
        int count = 0;

        do
        {
            count += 2;
        }while(ReadShort() != 0);

        return Encoding.Unicode.GetString(Bytes.Slice(start, count-2));
    }
    public string ReadStringDecrypted(int length) => Decrypt(Read(length));

    public T Read<T>() where T : IReadable, new()
    {
        var result = new T();
        result.Read(ref this);
        return result;
    }
    public static string Decrypt(ReadOnlySpan<byte> bytes)
    {
        int i = 0;
        int magic_var1 = 204;
        Span<byte> result = stackalloc byte[bytes.Length];
        foreach(var b in bytes)
        {
            int temp = b ^ 179;
            temp = magic_var1 - temp;
            magic_var1 = (byte)(magic_var1 + temp - i);
            
            result[i++] = (byte)temp;
        }

        return Encoding.ASCII.GetString(result);
}

    public void Dispose()
    {
    }
}
