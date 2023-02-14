using BlurFileEditor.Utils;
using BlurFileEditor.Windows;
using BlurFormats.BlurData;
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
    public int CurrentPage
    {
        get 
        { 
            return (int)GetValue(CurrentPageProperty); 
        }
        set 
        { 
            SetValue(CurrentPageProperty, value);
        }
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set { }
    }

    public int ItemsPerPage
    {
        get { return (int)GetValue(ItemsPerPageProperty); }
        set { SetValue(ItemsPerPageProperty, value); }
    }

    public IEnumerable<object> EntityItems
    {
        get => (IEnumerable<object>)GetValue(EntityItemsProperty);
        set 
        { 
            SetValue(EntityItemsProperty, value);
        }
    }

    public IEnumerable<object>? VisibleItems => ItemsPerPage < 0 ? EntityItems : EntityItems?.Skip(ItemsPerPage * CurrentPage)?.Take(ItemsPerPage);
    public ICommand InspectRecordCommand { get; }


    public static readonly DependencyProperty ItemsPerPageProperty =
        DependencyProperty.Register("ItemsPerPage", typeof(int), typeof(RecordTreeView), new FrameworkPropertyMetadata(5, (source, e) => { ((RecordTreeView)source).SetTotalPages(); }));

    public static readonly DependencyProperty EntityItemsProperty =
        DependencyProperty.Register("EntityItems", typeof(IEnumerable<object>), typeof(RecordTreeView), new FrameworkPropertyMetadata(Enumerable.Empty<object>(), (source, e) => { ((RecordTreeView)source).SetTotalPages(); }));

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register("CurrentPage", typeof(int), typeof(RecordTreeView), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (source, e) => 
        {
            var view = (RecordTreeView)source;
            view.SetTotalPages();
            view.UpdateProperty(nameof(CurrentPage));
            view.UpdateProperty(nameof(VisibleItems));
        }));

    protected static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register("TotalPages", typeof(int), typeof(RecordTreeView), new FrameworkPropertyMetadata(0));

    public RecordTreeView()
    {
        InitializeComponent();

        InspectRecordCommand = new Command<BlurRecord>((record) =>
        {
            if (record is null) return;
            new RecordInspector(record).Show();
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void SetTotalPages()
    {
        SetValue(TotalPagesProperty, (int)Math.Ceiling((EntityItems?.Count() ?? 0) / (double)ItemsPerPage));
    }
    public void UpdateProperty(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem m || m.DataContext is not BlurRecord record) return;
        new RecordInspector(record).Show();
    }
}
