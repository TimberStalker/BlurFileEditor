using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.ViewModels;
public abstract class ObservableObject : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public void UpdateProperty(string? property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

    public bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = "", Action? onChanging = null, Action? onChanged = null, Func<T, T, bool>? validateValue = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        if (validateValue is not null && !validateValue(backingStore, value))
        {
            return false;
        }

        onChanging?.Invoke();
        backingStore = value;
        onChanged?.Invoke();
        UpdateProperty(propertyName);
        return true;
    }
}
