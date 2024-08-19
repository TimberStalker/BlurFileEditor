using BlurFileEditor.ViewModels.Pages;
using BlurFormats.Loc;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlurFileEditor.Models.LocEditorPage;
public partial class LanguageItem : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsShowing))]
    bool enabled;
    public bool IsShowing => Enabled;
    [ObservableProperty]
    string name = "";
    [ObservableProperty]
    private LocEditorViewModel viewModel;
    [ObservableProperty]
    private IRelayCommand<LanguageItem> removeLanguageCommand;
    public event EventHandler<bool>? OnStateChanged;
    public LanguageItem(LocEditorViewModel viewModel, IRelayCommand<LanguageItem> removeLanguageCommand)
    {
        this.viewModel = viewModel;
        this.removeLanguageCommand = removeLanguageCommand;
        ViewModel = viewModel;
    }
    partial void OnEnabledChanged(bool oldValue, bool newValue)
    {
        OnStateChanged?.Invoke(this, newValue);
    }
}
