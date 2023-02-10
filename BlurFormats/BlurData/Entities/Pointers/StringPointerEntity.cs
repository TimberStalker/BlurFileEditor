using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities.Pointers;
public class StringPointerEntity : IEntity, IPointerEntity
{
    public int Offset { get; }
    public Guid Guid { get; }

    public DataType Type { get; }

    object IEntity.Value => Offset;
    internal StringPointerEntity(DataType dataType, int offset)
    {
        Guid = Guid.NewGuid();
        Type = dataType;
        Offset = offset;
    }


    public IEntity GetReplacement(PointerDataSource dataSource)
    {
        if (dataSource.TryGetData<char[]>(out var data))
        {
            return new PrimitiveEntity<string>(Type, GetTerminatedString(data, Offset));
        }
        return new NullEntity(Type);
    }

    public string GetTerminatedString(char[] chars, int offset)
    {
        if (offset >= chars.Length || chars[offset] == (char)0) return "";
        var builder = new StringBuilder();

        char nextchar;
        while ((nextchar = chars[offset + builder.Length]) != (char)0)
        {
            builder.Append(nextchar);
        }
        return builder.ToString();
    }
}
