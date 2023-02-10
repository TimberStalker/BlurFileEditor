using System;
using System.Collections.Generic;
using System.Linq;
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
}
