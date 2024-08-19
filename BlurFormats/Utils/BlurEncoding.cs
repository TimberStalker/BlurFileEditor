using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public class BlurEncoding : Encoding
{
    public int ChunkSize { get; set; } = 4;


    public override int GetByteCount(char[] chars, int index, int count)
    {
        if (index + count > chars.Length) throw new ArgumentOutOfRangeException();
        
        if (count % ChunkSize == 0) return count;
        return count + 4 - (count % ChunkSize);
    }

    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
    {
        int written = 0;
        int j = 0;
        int magic_var1 = 204;

        int totalBytes = charIndex + charCount;
        if (totalBytes % ChunkSize > 0) totalBytes += 4 - (totalBytes % ChunkSize);


        for (int i = charIndex; i < totalBytes; i++)
        {
            byte b = i >= chars.Length ? (byte)0 : (byte)chars[i];
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
        int accumulator = 204;
        for (int i = byteIndex; i < byteIndex + byteCount; i++)
        {
            byte b = bytes[i];

            byte originalChar = (byte)(accumulator - (b ^ 179));
            accumulator = (byte)(accumulator + originalChar - j);

            chars[charIndex + j++] = (char)originalChar;
            written++;
        }
        return written;
    }

    public override int GetMaxByteCount(int charCount)
    {
        if(charCount % ChunkSize == 0) return charCount;
        return charCount + 4 - (charCount % ChunkSize);
    }

    public override int GetMaxCharCount(int byteCount)
    {
        return byteCount;
    }

}
