using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Utils.Interop.Files;
public enum FileAttribute : uint
{
    Directory = 16,
    File = 256
}