using BlurFileEditor.Editors;
using BlurFileEditor.Utils.Dragging;
using BlurFileEditor.ViewModels.Windows;
using System;
using System.Collections.Generic;
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

namespace BlurFileEditor;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    readonly MainWindowViewModel viewModel;
    public MainWindow()
    {
        InitializeComponent();
        viewModel = (MainWindowViewModel)DataContext;
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if(WindowState == WindowState.Maximized)
        {
            WindowBorder.BorderThickness = new Thickness(8);
        }
        else
        {
            WindowBorder.BorderThickness = new Thickness(2);
        }
    }
    private void TabItem_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.Source is not TabItem tabItem) return;

        if (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
        {
            DragAdorner.StartDrag(tabItem);
            DragDrop.DoDragDrop(tabItem, tabItem, DragDropEffects.All);
            DragAdorner.EndDrag(tabItem);
        }
    }
    public void TabItem_Drop(object sender, DragEventArgs e)
    {
        if (e.Source is TabItem tabItemTarget &&
            e.Data.GetData(typeof(TabItem)) is TabItem tabItemSource &&
            !tabItemTarget.Equals(tabItemSource))
        {
            int sourceIndex = viewModel.OpenEditors.IndexOf((FileEditor)tabItemSource.DataContext);
            int targetIndex = viewModel.OpenEditors.IndexOf((FileEditor)tabItemTarget.DataContext);
            viewModel.MoveEditorTo(sourceIndex, targetIndex);
        }
    }
}
