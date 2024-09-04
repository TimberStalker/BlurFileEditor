using BlurFileFormats.Models.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.ViewModels.Pages;
public class ModelEditorViewModel : ObservableObject
{
    
    internal void SetInfo(FileSystemInfo info)
    {
        var model = CPModelSerializer.DeserializeModel(info.FullName);
    }
}
