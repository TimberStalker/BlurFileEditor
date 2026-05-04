using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.Projects;

public class BlurProject
{
    public string BaseDirectory { get; }

    public BlurProject(string baseDirectory)
    {
        BaseDirectory = baseDirectory;
    }
}
