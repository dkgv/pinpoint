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

        public PluginMeta(string name, PluginPriority priority, bool enabled = true) : this(name, string.Empty, priority, enabled)
        {
        }

        public PluginMeta(string name, string description, PluginPriority priority, bool enabled = true)
        {
            Name = name;
            Description = description;
            Priority = priority;
            Enabled = enabled;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public PluginPriority Priority { get; set; }

        public bool Enabled { get; set; }
    }
}
