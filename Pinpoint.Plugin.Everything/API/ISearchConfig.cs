namespace Pinpoint.Plugin.Everything.API
{
    public interface ISearchConfig
    {
        public bool RegexEnabled { get; set; }

        public bool MatchCase { get; set; }

        public uint MaxResults { get; set; }

        public RequestFlag RequestFlag { get; set; }

        public SortMethod SortMethod { get; set; }
    }
}
