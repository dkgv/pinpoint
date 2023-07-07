namespace Pinpoint.Core;

public class PluginManifest
{
    public PluginManifest()
    {
    }

    public PluginManifest(string name) : this(name, PluginPriority.Standard)
    {
    }

    public PluginManifest(string name, PluginPriority priority)
    {
        Name = name;
        Priority = priority;
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public PluginPriority Priority { get; set; }

    public bool Enabled { get; set; }
}
