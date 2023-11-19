using Microsoft.Toolkit.Mvvm.ComponentModel;
using Pinpoint.Plugin;

namespace Pinpoint.Win.ViewModels;

public class PluginTabItemViewModel : ObservableObject
{
    private AbstractPlugin _plugin;

    public AbstractPlugin Plugin
    {
        get => _plugin;
        set => SetProperty(ref _plugin, value);
    }

    public string StatusColor => Enabled ? "LightGreen" : "Red";

    public bool Enabled
    {
        get => Plugin.State.IsEnabled.Value;
        set
        {
            Plugin.State.IsEnabled = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(ToggleEnabledString));
        }
    }

    public string ToggleEnabledString => Enabled ? "Disable" : "Enable";
}