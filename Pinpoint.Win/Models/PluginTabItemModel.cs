using Pinpoint.Core;

namespace Pinpoint.Win.Models
{
    internal class PluginTabItemModel : BaseControlModel
    {
        private IPlugin _plugin;
        
        public IPlugin Plugin
        {
            get => _plugin;
            set => SetProperty(ref _plugin, value);
        }
    }
}