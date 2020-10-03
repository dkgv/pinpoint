using System.IO;

namespace Pinpoint.Core.Snippets
{
    public class FileSnippet : ISnippet
    {
        private string _rawContent;

        public FileSnippet()
        {
        }

        public FileSnippet(string filePath)
        {
            FilePath = filePath;
            Identifier = Path.GetFileName(filePath);
            RawContent = File.ReadAllText(filePath);
        }

        public string RawContent
        {
            get
            {
                if (string.IsNullOrEmpty(_rawContent))
                {
                    _rawContent = File.ReadAllText(FilePath);
                }

                return _rawContent;
            }
            set => _rawContent = value;
        }

        public string Identifier { get; set; }

        public string FilePath { get; set; }

        public void SaveAsJSON(bool overwrite = true)
        {
            throw new System.NotImplementedException();
        }

        public void SaveAsMarkdown()
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            return obj is FileSnippet src && src.FilePath.Equals(FilePath);
        }
    }
}
