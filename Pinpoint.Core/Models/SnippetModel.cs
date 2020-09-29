namespace Pinpoint.Core.Models
{
    public class SnippetModel
    {
        public SnippetModel(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public string Title { get; set; }

        public string Content { get; set; }
    }
}
