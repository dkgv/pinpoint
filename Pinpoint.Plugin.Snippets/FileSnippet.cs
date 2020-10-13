using System.IO;

namespace Pinpoint.Plugin.Snippets
{
    public sealed class FileSnippet : AbstractSnippet
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

        public new string RawContent
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

        public override string FilePath { get; set; }

        public override void SaveAsJson(bool overwrite = true)
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            return obj is FileSnippet src && src.FilePath.Equals(FilePath);
        }
    }
}
