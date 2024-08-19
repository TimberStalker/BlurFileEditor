using BlurFileEditor.Utils;
using BlurFileEditor.Utils.FIlter;
using BlurFormats.Serialization;
using BlurFormats.Serialization.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BlurFileEditor.ViewModels.Windows
{
    public class RecordInspectorViewModel : ObservableObject
    {
        object? selectedItem;
        SerializationRecord? record;
        IEntityFilter? filter;
        public SerializationRecord? Record 
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
                if(SelectedItem is IEntity e)
                {
                    Filter = new SpecificEntityFilter() { Entity = e };
                } else if(SelectedItem is ArrayEntityItem a)
                {
                    Filter = new SpecificEntityFilter() { Entity = ((ReferenceEntity)a.Reference).Reference };
                } else if(SelectedItem is StructureFieldValue s)
                {
                    if(s.Entity is ReferenceEntity r)
                    {
                        Filter = new SpecificEntityFilter() { Entity = r.Reference };
                    }
                    else
                    {
                        Filter = new SpecificEntityFilter() { Entity = s.Entity };
                    }
                }
            } 
        }
        public IEntityFilter? Filter
        {
            get => filter;
            set
            {
                filter = value;
                UpdateProperty(nameof(Filter));
            }
        }

        public ICommand SelectedItemChanged { get; }
        public RecordInspectorViewModel()
        {
            SelectedItemChanged = new Command<object>(o =>
            {
                SelectedItem = o;
            });
        }
    }
}
