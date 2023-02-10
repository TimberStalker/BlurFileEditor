using BlurFormats.BlurData.Read;
using BlurFormats.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData;
public class BlurData
{
    public static BlurData FromBytes(byte[] bytes)
    {
        var reader = new Reader(bytes);
        var read = DataRead.FromBytes(ref reader);

        return null;
    }
}
