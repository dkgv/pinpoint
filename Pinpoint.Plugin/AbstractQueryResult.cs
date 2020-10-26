using FontAwesome5;

namespace Pinpoint.Plugin
{
    public abstract class AbstractQueryResult
    {
        protected AbstractQueryResult()
        {
        }

        protected AbstractQueryResult(string title, string subtitle = "")
        {
            Title = title;
            Subtitle = subtitle;
        }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public object Instance { get; }

        public string Shortcut { get; set; }

        public abstract EFontAwesomeIcon Icon { get; }

        public abstract void OnSelect();
    }
}
