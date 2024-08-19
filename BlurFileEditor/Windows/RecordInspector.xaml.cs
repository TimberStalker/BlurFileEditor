using BlurFileEditor.ViewModels.Windows;
using BlurFormats.Serialization;
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
using System.Windows.Shapes;

namespace BlurFileEditor.Windows
{
    /// <summary>
    /// Interaction logic for RecordInspector.xaml
    /// </summary>
    public partial class RecordInspector : Window
    {
        public RecordInspector(SerializationRecord record)
        {
            InitializeComponent();
            ((RecordInspectorViewModel)DataContext).Record = record;
        }
    }
}
