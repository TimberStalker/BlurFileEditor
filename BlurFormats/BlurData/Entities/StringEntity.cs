using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlurFormats.BlurData.Entities.StringEntity;

namespace BlurFormats.BlurData.Entities;
public class StringEntity : PrimitiveEntity<StringContainer>
{
    int stringOffset;
    public StringEntity(DataType type, string value) : base(type, new StringContainer(value))
    {
        stringOffset = -1;
    }
    public StringEntity(DataType type, int stringOffset) : base(type, new StringContainer(""))
    {
        this.stringOffset = stringOffset;
    }
    public void GetStringFromOffset(char[] characters)
    {
        Value = new StringContainer(characters.GetTerminatedString(stringOffset));
    }
}
public class StringContainer
{
    public string Value { get; set; }
    public StringContainer(string value)
    {
        Value = value;
    }
}
