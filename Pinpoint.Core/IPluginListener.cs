using Pinpoint.Plugin;

namespace Pinpoint.Core
{
    public interface IPluginListener<in Plugin, in Target> where Plugin : AbstractPlugin
    {
        void PluginChange_Added(object sender, Plugin plugin, Target target);

        void PluginChange_Removed(object sender, Plugin plugin, Target target);
    }
}
