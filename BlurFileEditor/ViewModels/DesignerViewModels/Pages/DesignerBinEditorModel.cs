using BlurFileEditor.ViewModels.Pages;
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
        this.SetFileSource(new FileInfo("C:\\Users\\MyName\\Desktop\\BlurModified\\gamedata\\xt\\achievements.bin"));
    }
}
