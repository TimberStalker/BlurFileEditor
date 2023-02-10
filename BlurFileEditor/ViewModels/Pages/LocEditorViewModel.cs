using BlurFileEditor.Utils;
using BlurFormats.Loc;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BlurFileEditor.ViewModels.Pages;
public class LocEditorViewModel : ViewModelBase
{

    OpenFileDialog openDialog;
    SaveFileDialog saveDialog;
    public Localization Localization { get; set; }
    public ICommand LoadFile { get; private set; }
    public ICommand ExportFile { get; private set; }
    public ICommand AddLanguage { get; private set; }
    public ICommand RemoveLanguage { get; private set; }
    public LocEditorViewModel()
    {
        openDialog = new OpenFileDialog()
        { 
            Filter = "Loc (OLTX) | *.loc"
        };
        saveDialog = new SaveFileDialog()
        {
            Filter = "Loc (OLTX) | *.loc"
        };

        Localization = new Localization();
        LoadFile = new Command(() =>
        {
            if (openDialog.ShowDialog() == true)
            {
                using var stream = openDialog.OpenFile();
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);

                try
                {
                    Localization = Localization.Create(bytes);
                    OnPropertyChanged(nameof(Localization));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        });
        ExportFile = new Command(() =>
        {
            if(saveDialog.ShowDialog() == true)
            {
                var bytes = Localization.ToBytes(Localization);
                File.WriteAllBytes(saveDialog.FileName, bytes);
            }
        });
        AddLanguage = new Command(() =>
        {
            Localization.Languages.Add(new Localization.Language());
            OnPropertyChanged("Localization.Languages");
        });
        RemoveLanguage = new Command<Localization.Language>((l) =>
        {
            Localization.Languages.Remove(l);
            OnPropertyChanged("Localization.Languages");
        });
    }
}
