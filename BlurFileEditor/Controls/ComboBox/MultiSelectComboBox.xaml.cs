using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlurFileEditor.Controls.ComboBox;
/// <summary>
/// Interaction logic for MultiSelectComboBox.xaml
/// </summary>
public partial class MultiSelectComboBox : UserControl, INotifyPropertyChanged
{

    public int SelectedIndex 
    {
        get => -1;
        set
        {
            if (value < 0) return;
            SelectedIndicies ^= 1 << value;
        }
    }
    public int SelectedIndicies
    {
        get { return (int)GetValue(SelectedIndiciesProperty); }
        set 
        {
            SetValue(SelectedIndiciesProperty, value); 
            OnPropertyChanged(nameof(SelectedIndicies));
        }
    }

    public IEnumerable ItemsSource
    {
        get { return (IEnumerable)GetValue(ItemsSourceProperty); }
        set 
        { 
            SetValue(ItemsSourceProperty, value); 
            OnPropertyChanged(nameof(ItemsSource));
        }
    }

    public static readonly DependencyProperty SelectedIndiciesProperty =
        DependencyProperty.Register("SelectedIndicies", typeof(int), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(0));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(Enumerable.Empty<object>()));

    public event PropertyChangedEventHandler? PropertyChanged;

    public MultiSelectComboBox()
    {
        InitializeComponent();
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }

    public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
