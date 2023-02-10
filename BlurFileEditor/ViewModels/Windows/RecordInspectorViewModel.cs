using BlurFormats.BlurData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.ViewModels.Windows
{
    public class RecordInspectorViewModel : ViewModelBase
    {
        BlurRecord? record;
        public BlurRecord? Record 
        { 
            get => record;
            set
            {
                record = value;
                UpdateProperty(nameof(BlurRecord));
            }
        }
        public RecordInspectorViewModel()
        {

        }
    }
}
