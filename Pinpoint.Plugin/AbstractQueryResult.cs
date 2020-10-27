using System.Drawing;

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

        public abstract Bitmap Icon { get; }

        /// <summary>
        /// Fired when result is selected (double-clicked or when "ENTER" is pressed) from list.
        /// </summary>
        public abstract void OnSelect();
    }
}
