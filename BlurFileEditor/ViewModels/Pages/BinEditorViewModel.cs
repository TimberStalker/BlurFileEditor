using BlurFileEditor.Utils;
using BlurFormats.BlurData;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BlurFileEditor.ViewModels.Pages;
public class BinEditorViewModel : ViewModelBase
{
    FileSystemInfo? info;
    string binTypesText = "";

    public BlurData Bin { get; set; }
    public string BinTypesText
    {
        get => binTypesText;
        set {
            binTypesText = value;
            UpdateProperty(nameof(BinTypesText));
        }
    }
    public BinEditorViewModel()
    {
        Bin = new BlurData();
    }
    public void SetFileSource(FileSystemInfo info)
    {
        this.info = info;
        using var stream = new FileStream(info.FullName, FileMode.Open);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        var block = BlurData.FromBytes(bytes);

        using var textStream = new StringWriter();
        block.PrintTypes(textStream);

        Bin = block;
        BinTypesText = textStream.ToString();
        UpdateProperty(nameof(Bin));
    }
}
