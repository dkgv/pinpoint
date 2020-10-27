using System;
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

        protected bool Equals(AbstractQueryResult other)
        {
            return Title == other.Title && Subtitle == other.Subtitle && Equals(Instance, other.Instance);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((AbstractQueryResult) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Title, Subtitle, Instance);
        }
    }
}
