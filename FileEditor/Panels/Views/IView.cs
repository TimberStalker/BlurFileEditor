using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor.Panels.Views;
public interface IView
{
    bool Changed => false;
    void Draw();
    void HandleKeybinds() { }
}
