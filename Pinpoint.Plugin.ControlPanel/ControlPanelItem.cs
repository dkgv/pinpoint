namespace Pinpoint.Plugin.ControlPanel
{
    public class ControlPanelItem
    {
        public ControlPanelItem()
        {
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string CanonicalName { get; set; }

        public string[] Category { get; set; }

        public string RegistryKey { get; set; }
    }
}