namespace Pinpoint.Plugin.Everything.API
{
    public class DefaultSearchConfig : ISearchConfig
    {
        public bool RegexEnabled { get; set; } = false;

        public bool MatchCase { get; set; } = false;

        public uint MaxResults { get; set; } = 16;

        public RequestFlag RequestFlag { get; set; } = RequestFlag.FileName | RequestFlag.Path | RequestFlag.Extension;
        
        public SortMethod SortMethod { get; set; } = SortMethod.ByDateModifiedDescending;
    }
}
