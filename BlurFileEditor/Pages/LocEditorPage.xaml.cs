using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
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
/// Interaction logic for LocEditor.xaml
/// </summary>
public partial class LocEditorPage : Page
{
    public LocEditorPage()
    {
        InitializeComponent();
    }
    public LocEditorPage(FileSystemInfo info) : this()
    {
    }
}
