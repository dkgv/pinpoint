using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Pinpoint.Core;

namespace Pinpoint.Win.ViewModels
{
    public class PluginTabItemViewModel : ObservableObject
    {
        private IPlugin _plugin;

        public IPlugin Plugin
        {
            get => _plugin;
            set => SetProperty(ref _plugin, value);
        }

        public string StatusColor => Enabled ? "LightGreen" : "Red";

        public bool Enabled
        {
            get => Plugin.Meta.Enabled;
            set
            {
                Plugin.Meta.Enabled = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StatusColor));
                OnPropertyChanged(nameof(ToggleEnabledString));
            }
        }

        public string ToggleEnabledString => Enabled ? "Disable" : "Enable";

        public IRelayCommand ToggleEnabledCommand => new RelayCommand(() =>
        {
            Enabled = !Enabled;
        });
    }
}