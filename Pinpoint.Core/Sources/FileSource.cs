using System.IO;

namespace Pinpoint.Core.Sources
{
    public class FileSource : ISource
    {
        private string _rawContent;

        public FileSource()
        {
        }

        public FileSource(string filePath)
        {
            Location = filePath;
            Identifier = Path.GetFileName(filePath);
            RawContent = File.ReadAllText(filePath);
        }

        public string RawContent
        {
            get
            {
                if (string.IsNullOrEmpty(_rawContent))
                {
                    _rawContent = File.ReadAllText(Location);
                }

                return _rawContent;
            }
            set => _rawContent = value;
        }

        public string Identifier { get; set; }

        public string Location { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is FileSource src && src.Location.Equals(Location);
        }
    }
}
