namespace Pinpoint.Core
{
    public class PluginMeta
    {
        public PluginMeta()
        {
        }

        public PluginMeta(string name, bool enabled = true) : this(name, PluginPriority.Standard, enabled)
        {
            Name = name;
            Enabled = enabled;
        }
        public PluginMeta(string name, PluginPriority priority, bool enabled = true)
        {
            Name = name;
            Priority = priority;
            Enabled = enabled;
        }

        public string Name { get; set; }

        public PluginPriority Priority { get; set; }

        public bool Enabled { get; set; }
    }
}
