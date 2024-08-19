using BlurFileEditor.Utils;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BlurFileEditor.Models.LocEditorPage.LocalizationItem;
using static BlurFormats.Loc.BlurLocalization;

namespace BlurFileEditor.Models.LocEditorPage;
public partial class LocalizationItem : ObservableRecipient
{
    [ObservableProperty]
    uint id;
    [ObservableProperty]
    string header = "";
    public LocalizationTextItem? FirstEnabledTextItem => LocalizationTexts.FirstOrDefault(t => t.Language.IsShowing);
    public bool IsSingleLanguageActive => LocalizationTexts.Count(t => t.Language.IsShowing) == 1;

    public ObservableCollection<LocalizationTextItem> LocalizationTexts { get; } = new ObservableCollection<LocalizationTextItem>();
    public ObservableFilterCollection<ObservableCollection<LocalizationTextItem>, LocalizationTextItem> FilteredTexts { get; }
    public LocalizationItem(IEnumerable<KeyValuePair<LanguageItem, string>> localizations)
    {
        foreach (var (language, text) in localizations)
        {
            AddText(language, text);
        }
        LocalizationTexts.CollectionChanged += LocalizationTexts_CollectionChanged;
        FilteredTexts = new ObservableFilterCollection<ObservableCollection<LocalizationTextItem>, LocalizationTextItem>(LocalizationTexts);
        FilteredTexts.Filters += (sender, e) =>
        {
            e.Accepted = e.Item.Language?.IsShowing == true;
        };
    }

    private void LocalizationTexts_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            if(e.OldItems is not null)
            {
                foreach (LocalizationTextItem item in e.OldItems)
                {
                    item.PropertyChanged -= Item_PropertyChanged;
                }
            }
        }
    }

    public LocalizationTextItem AddText(LanguageItem language, string text)
    {
        var item = new LocalizationTextItem(language, this) { Text = text };
        item.PropertyChanged += Item_PropertyChanged;
        LocalizationTexts.Add(item);
        return item;
    }

    private void Item_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsSingleLanguageActive));
        OnPropertyChanged(nameof(LocalizationTexts));
    }

    ~LocalizationItem()
    {
        LocalizationTexts.CollectionChanged -= LocalizationTexts_CollectionChanged;
    }

    public partial class LocalizationTextItem : ObservableObject
    {
        [ObservableProperty]
        LanguageItem language;
        [ObservableProperty]
        string text = "";
        [ObservableProperty]
        LocalizationItem parent;
        public LocalizationTextItem(LanguageItem language, LocalizationItem parent)
        {
            Parent = parent;
            Language = language;

            Language.PropertyChanged += Language_PropertyChanged;
        }

        private void Language_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(Language));
        }

        ~LocalizationTextItem()
        {
            Language.PropertyChanged -= Language_PropertyChanged;
        }
    }
}
