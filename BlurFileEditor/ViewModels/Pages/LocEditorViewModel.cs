﻿using BlurFileEditor.Utils;
using BlurFormats.Loc;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static BlurFormats.Loc.BlurLocalization;

namespace BlurFileEditor.ViewModels.Pages;
public class LocEditorViewModel : ViewModelBase
{
    FileSystemInfo? info;
    string searchCriteria = "";
    public string SearchCriteria
    {
        get => searchCriteria;
        set {
            searchCriteria = value;
            UpdateProperty(nameof(VisibleStrings));
        }
    }
    public IEnumerable<LocString> VisibleStrings => string.IsNullOrEmpty(SearchCriteria) ? Localization.Strings : Localization.Strings.Where(t => t.Header.Contains(SearchCriteria, StringComparison.CurrentCultureIgnoreCase) || t.Texts.Any(kv => kv.Value.Text.Contains(SearchCriteria, StringComparison.CurrentCultureIgnoreCase)));
    public BlurLocalization Localization { get; set; }
    public ICommand AddLanguage { get; private set; }
    public ICommand RemoveLanguage { get; private set; }
    public LocEditorViewModel()
    {
        Localization = new BlurLocalization();
        //ExportFile = new Command(() =>
        //{
        //    if(saveDialog.ShowDialog() == true)
        //    {
        //        var bytes = Localization.ToBytes(Localization);
        //        File.WriteAllBytes(saveDialog.FileName, bytes);
        //    }
        //});
        AddLanguage = new Command(() =>
        {
            Localization.Languages.Add(new BlurLocalization.Language());
            UpdateProperty("Localization");
        });
        RemoveLanguage = new Command<BlurLocalization.Language>((l) =>
        {
            if (l is null) return;

            Localization.Languages.Remove(l);
            UpdateProperty("Localization");
        });
    }
    public void SaveToFile()
    {
        if (info is null) return;

        var bytes = BlurLocalization.ToBytes(Localization);
        File.WriteAllBytes(info.FullName, bytes);
    }
    public void SetFileSource(FileSystemInfo info)
    {
        this.info = info;
        using var stream = new FileStream(info.FullName, FileMode.Open);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        try
        {
            Localization = BlurLocalization.CreateFrom(bytes);
            UpdateProperty(nameof(Localization));
            UpdateProperty(nameof(VisibleStrings));
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message);
        }
    }
}
