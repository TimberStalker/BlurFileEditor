using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFormats.BlurData.Entities;
public abstract class Entity : INotifyPropertyChanged
{
    public DataType DataType { get; set; }
    public object Value { get; set; }
    public Range Range { get; set; }
    public Type EntityType => this.GetType();
    public Entity(DataType dataType, object value)
    {
        DataType = dataType;
        Value = value;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void UpdateProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public override string ToString() => $"{DataType.Name} : {Value}";
}
