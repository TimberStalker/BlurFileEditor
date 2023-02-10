using BlurFileEditor.ViewModels.Pages;
using BlurFormats.BinFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace BlurFileEditor.ViewModels.DesignerViewModels.Pages;
internal class DesignerBinEditorModel : BinEditorViewModel
{
	public DesignerBinEditorModel()
	{

        Bin = BinBlock.FromBytes(File.ReadAllBytes("C:\\Users\\MyName\\Desktop\\OriginalBlur\\Blur\\cache\\gamedata\\gamedata\\xt\\multiplayer.bin"));

        using var textStream = new StringWriter();
        Bin.PrintTypes(textStream);

        BinTypesText = textStream.ToString();
    }
}
