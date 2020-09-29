namespace Pinpoint.Core.Sources
{
    public class ManualSource : ISource
    {
        public ManualSource(string identifier, string rawContent, string location)
        {
            Identifier = identifier;
            RawContent = rawContent;
            Location = location;
        }

        public string RawContent { get; set; }

        public string Identifier { get; set; }
        
        public string Location { get; set; }
    }
}
