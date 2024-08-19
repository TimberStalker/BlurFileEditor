using BlurFileEditor.Utils;
using BlurFileEditor.Utils.FIlter;
using BlurFileEditor.Windows;
using BlurFormats.Serialization;
using System;
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

namespace BlurFileEditor.Controls;
/// <summary>
/// Interaction logic for RecordTreeView.xaml
/// </summary>
public partial class RecordTreeView : UserControl, INotifyPropertyChanged
{

    public object SelectedItem
    {
        get 
        { 
            return (object)GetValue(SelectedItemProperty); 
        }
        set 
        { 
            if(SelectedItem != value)
            {
                SetValue(SelectedItemProperty, value);
                SelectedItemChangedCommand?.Execute(SelectedItem);
            }
        }
    }

    public IEnumerable<object> EntityItems
    {
        get => (IEnumerable<object>)GetValue(EntityItemsProperty);
        set 
        { 
            SetValue(EntityItemsProperty, value);
        }
    }
    public ICommand InspectRecordCommand { get; }

    public Brush HighlightBrush
    {
        get { return (Brush)GetValue(HighlightBrushProperty); }
        set { SetValue(HighlightBrushProperty, value); }
    }


    public IEntityFilter HighlightFilter
    {
        get { return (IEntityFilter)GetValue(HighlightFilterProperty); }
        set { SetValue(HighlightFilterProperty, value); }
    }

    public ICommand SelectedItemChangedCommand
    {
        get { return (ICommand)GetValue(SelectedItemChangedCommandProperty); }
        set { SetValue(SelectedItemChangedCommandProperty, value); }
    }

    public static readonly DependencyProperty SelectedItemChangedCommandProperty =
        DependencyProperty.Register("SelectedItemChangedCommand", typeof(ICommand), typeof(RecordTreeView), new FrameworkPropertyMetadata(null));



    public static readonly DependencyProperty HighlightFilterProperty =
        DependencyProperty.Register("HighlightFilter", typeof(IEntityFilter), typeof(RecordTreeView), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register("SelectedItem", typeof(object), typeof(RecordTreeView), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty HighlightBrushProperty =
        DependencyProperty.Register("HighlightColor", typeof(Brush), typeof(RecordTreeView), new FrameworkPropertyMetadata(new SolidColorBrush(Colors.Transparent)));

    public static readonly DependencyProperty EntityItemsProperty =
        DependencyProperty.Register("EntityItems", typeof(IEnumerable<object>), typeof(RecordTreeView), new FrameworkPropertyMetadata(Enumerable.Empty<object>()));

    public RecordTreeView()
    {
        InitializeComponent();

        InspectRecordCommand = new Command<SerializationRecord>((record) =>
        {
            if (record is null) return;
            new RecordInspector(record).Show();
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void UpdateProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem m || m.DataContext is not SerializationRecord record) return;
        new RecordInspector(record).Show();
    }

    private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedItem = e.NewValue;
    }
}
