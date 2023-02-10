using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public class BlurEncoding : Encoding
{
    public override int GetByteCount(char[] chars, int index, int count)
    {
        return count;
    }

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        int written = 0;
        int j = 0;
        int magic_var1 = 204;
        for (int i = charIndex; i < charIndex + charCount; i++)
        {
            byte b = bytes[i];
            byte enryptedChar = (byte)((magic_var1 - b) ^ 179);
            magic_var1 = (byte)(magic_var1 + b - j);

            bytes[byteIndex + j++] = enryptedChar;
            written++;
        }
        return written;
    }

    public override int GetCharCount(byte[] bytes, int index, int count)
    {
        if (index + count > bytes.Length) throw new ArgumentOutOfRangeException();
        return count;
    }

    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
    {
        int written = 0;
        int j = 0;
        int magic_var1 = 204;
        for (int i = byteIndex; i < byteIndex + byteCount; i++)
        {
            byte b = bytes[i];

            byte originalChar = (byte)(magic_var1 - (b ^ 179));
            magic_var1 = (byte)(magic_var1 + originalChar - j);

            chars[charIndex + j++] = (char)originalChar;
            written++;
        }
        return written;
    }

    public static string Decrypt(ReadOnlySpan<byte> bytes)
    {
        int i = 0;
        int magic_var1 = 204;
        Span<byte> result = stackalloc byte[bytes.Length];
        foreach (var b in bytes)
        {
            byte originalChar = (byte)(magic_var1 - (b ^ 179));
            magic_var1 = (byte)(magic_var1 + originalChar - i);

            result[i++] = originalChar;
        }

        return Encoding.ASCII.GetString(result);
    }
    public static Span<byte> Encrypt(ReadOnlySpan<byte> bytes)
    {
        int i = 0;
        int magic_var1 = 204;
        byte[] result = new byte[bytes.Length];
        foreach (var b in bytes)
        {
            byte enryptedChar = (byte)((magic_var1 - b) ^ 179);
            magic_var1 = (byte)(magic_var1 + b - i);

            result[i++] = enryptedChar;
        }

        return result;
    }

    public override int GetMaxByteCount(int charCount)
    {
        return charCount;
    }

    public override int GetMaxCharCount(int byteCount)
    {
        return byteCount;
    }

}
