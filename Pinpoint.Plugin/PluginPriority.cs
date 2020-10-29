namespace Pinpoint.Plugin
{
    public enum PluginPriority
    {
        Highest = int.MaxValue, 
        NextHighest = 100,
        Standard = 0,
        NextLowest = -100,
        Lowest = int.MinValue
    }
}