using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public class ObjectEntity : Entity
{
    public ObservableCollection<Entity> Fields => (ObservableCollection<Entity>)Value;
    public ObjectEntity(DataType dataType) : base(dataType, new ObservableCollection<Entity>())
    {
    }
}
