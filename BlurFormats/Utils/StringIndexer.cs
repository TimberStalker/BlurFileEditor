using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.Utils;
public class StringIndexer
{
    Dictionary<string, int> indecies = new Dictionary<string, int>();
    StringBuilder builder = new StringBuilder();

    public int GetIndex(string text)
    {
        if(indecies.TryGetValue(text, out int index)) return index;
        indecies[text] = index = builder.Length;
        builder.Append(text);
        builder.Append('\0');
        return index;
    }

    public override string ToString()
    {
        return builder.ToString();
    }
}
