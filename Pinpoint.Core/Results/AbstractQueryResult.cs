using System;
using System.Collections.Generic;
using System.Drawing;

namespace Pinpoint.Core.Results
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

        public List<AbstractQueryResult> Options { get; set; } = new List<AbstractQueryResult>();

        public string Title { get; set; }

        public string Subtitle { get; set; }
        
        public string Shortcut { get; set; }

        public abstract Bitmap Icon { get; }

        /// <summary>
        /// Fired when result is selected (double-clicked or when "ENTER" is pressed) from list.
        /// </summary>
        public abstract void OnSelect();

        public virtual bool OnPrimaryOptionSelect() => false;

        protected bool Equals(AbstractQueryResult other)
        {
            return Title == other.Title && Subtitle == other.Subtitle;
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
            return HashCode.Combine(Title, Subtitle);
        }
    }
}
