using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public static class Extensions
{
    public static string GetTerminatedStringAtOffset(this string text, int offset)
    {
        if (offset >= text.Length || text[offset] == (char)0) return "";
        int length = 1;
        while (text[offset + length] != (char)0)
        {
            length++;
        }
        return text.Substring(offset, length);
    }
    public static string GetTerminatedString(this char[] chars, int offset)
    {
        if (offset < 0 || offset >= chars.Length || chars[offset] == (char)0) return "";
        var builder = new StringBuilder();

        char nextchar;
        while ((nextchar = chars[offset + builder.Length]) != (char)0)
        {
            builder.Append(nextchar);
        }
        return builder.ToString();
    }
}
