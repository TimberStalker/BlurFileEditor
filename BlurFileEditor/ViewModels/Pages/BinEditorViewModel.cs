using BlurFileEditor.Utils;
using BlurFormats.BinFile;
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
    OpenFileDialog fileDialog;
    string binTypesText = "";

    public BinBlock Bin { get; set; }
    public string BinTypesText
    {
        get => binTypesText;
        set {
            binTypesText = value;
            OnPropertyChanged(nameof(BinTypesText));
        }
    }
    public ICommand LoadFile { get; private set; }
    public BinEditorViewModel()
    {
        Bin = new BinBlock();
        fileDialog = new OpenFileDialog()
        {
            Filter = "Bin | *.bin"
        };
        LoadFile = new Command(() =>
        {
            if (fileDialog.ShowDialog() == true)
            {
                using var stream = fileDialog.OpenFile();
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                var block = BinBlock.FromBytes(bytes);

                using var textStream = new StringWriter();
                block.PrintTypes(textStream);

                Bin = block;
                BinTypesText = textStream.ToString();
            }
        });
    }
}
