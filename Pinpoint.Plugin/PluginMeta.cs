﻿namespace Pinpoint.Plugin
{
    public class PluginMeta
    {
        public PluginMeta(string name, bool enabled = true)
        {
            Name = name;
            Enabled = enabled;
        }

        public string Name { get; set; }

        public bool Enabled { get; set; }
    }
}
