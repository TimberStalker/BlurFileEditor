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
        object? selectedItem;
        BlurRecord? record;
        public BlurRecord? Record 
        { 
            get => record;
            set
            {
                record = value;
                UpdateProperty(nameof(Record));
            }
        }
        public object? SelectedItem 
        { 
            get => selectedItem; 
            set 
            { 
                selectedItem = value; 
                UpdateProperty(nameof(SelectedItem)); 
            } 
        }
        public RecordInspectorViewModel()
        {

        }
    }
}
