using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils;
public class Flag
{
    public bool IsSet { get; private set; }

    public void Set() => IsSet = true;
    public void Unset() => IsSet = false;
    public bool Consume() => IsSet && !(IsSet = false);
}
