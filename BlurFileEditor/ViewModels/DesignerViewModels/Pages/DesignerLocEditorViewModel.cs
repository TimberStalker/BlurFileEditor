using BlurFileEditor.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using BlurFormats.Loc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.ViewModels.DesignerViewModels.Pages;
internal class DesignerLocEditorViewModel : LocEditorViewModel
{
	public DesignerLocEditorViewModel()
	{
		Localization = Localization.CreateFrom(File.ReadAllBytes("C:\\Users\\ChrisG\\Desktop\\OriginalBlur\\Blur\\cache\\gamedata\\gamedata\\xt\\loc\\multiplayerts.loc"));
	}
}
