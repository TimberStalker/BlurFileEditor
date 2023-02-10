using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ArrayEntity : Entity
{
    new public ObservableCollection<Entity> Value => (ObservableCollection<Entity>)base.Value;
    public ArrayEntity(DataType dataType) : base(dataType, new ObservableCollection<Entity>())
    {
    }

    public override string ToString() => $"{DataType.Name}[] {DataField?.Name} = {Value}";
}
