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

namespace BlurFileEditor.Pages;
/// <summary>
/// Interaction logic for BinEditor.xaml
/// </summary>
public partial class BinEditorPage : Page
{
    public BinEditorPage()
    {
        InitializeComponent();
    }

    private void ComboBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled= true;
    }
}
