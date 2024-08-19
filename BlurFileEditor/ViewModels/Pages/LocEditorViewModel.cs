using BlurFileEditor.Models.LocEditorPage;
using BlurFileEditor.Utils;
using BlurFormats.Loc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static BlurFormats.Loc.BlurLocalization;

namespace BlurFileEditor.ViewModels.Pages;
public partial class LocEditorViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    FileSystemInfo? info;

    [ObservableProperty]
    string searchCriteria = "";

    public ObservableCollection<LanguageItem> Languages { get; } = new ObservableCollection<LanguageItem>();
    public ObservableCollection<LocalizationItem> LocalizationStrings { get; } = new ObservableCollection<LocalizationItem>();
    //public IEnumerable<LocString> VisibleStrings => string.IsNullOrEmpty(SearchCriteria) ? Localization.Strings : Localization.Strings.Where(t => t.Header.Contains(SearchCriteria, StringComparison.CurrentCultureIgnoreCase) || t.Texts.Any(kv => kv.Value.Text.Contains(SearchCriteria, StringComparison.CurrentCultureIgnoreCase)));
    public LocEditorViewModel()
    {
    }
    public void SaveToFile()
    {
        if (info is null) return;

        //var bytes = BlurLocalization.ToBytes(Localization);
        //File.WriteAllBytes(info.FullName, bytes);
    }
    [RelayCommand]
    public void AddLanguage()
    {
        var language = CreateLanguageItem("");
        Languages.Add(language);
        foreach (var item in LocalizationStrings)
        {
            item.AddText(language, "");
        }
    }
    [RelayCommand]
    public void RemoveLanguage(LanguageItem languageItem)
    {
        foreach (var item in LocalizationStrings)
        {
            var language = item.LocalizationTexts.FirstOrDefault(i => i.Language == languageItem);
            if(language is not null)
            {
                item.LocalizationTexts.Remove(language);
            }
        }
        Languages.Remove(languageItem);
    }
    public LanguageItem CreateLanguageItem(string name)
    {
        var item = new LanguageItem(this, RemoveLanguageCommand) { Name = name };
        return item;
    }
    public void SetFileSource(FileSystemInfo info)
    {
        this.info = info;
        using var stream = new FileStream(info.FullName, FileMode.Open);
        var bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        try
        {
            var localization = BlurLocalization.CreateFrom(bytes);
            foreach (var language in localization.Languages)
            {
                Languages.Add(CreateLanguageItem( language.Name ));
            }
            foreach (var localizationString in localization.Strings)
            {
                LocalizationStrings.Add(new LocalizationItem(localizationString.Texts.Select(kv => new KeyValuePair<LanguageItem, string>(Languages.First(l => l.Name == kv.Key.Name), kv.Value))) 
                { 
                    Header = localizationString.Header, 
                    Id = localizationString.Id 
                });
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(ex.Message);
        }
    }
}
public class LocalizationItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate? SingleLanguageTemplate { get; set; }
    public DataTemplate? MultiLanguageTemplate { get; set; }
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is not LocalizationItem l) throw new InvalidCastException();
        
        if(l.LocalizationTexts.Count(t => t.Language.IsShowing) == 1)
        {
            return SingleLanguageTemplate;
        }
        return MultiLanguageTemplate;
    }
}