using System;

namespace Pinpoint.Core
{
    public interface IQueryResult : IComparable<IQueryResult>
    {
        float Accuracy { get; }

        string Content { get; }

        string Location { get; }

        int IComparable<IQueryResult>.CompareTo(IQueryResult other)
        {
            return other.Accuracy.CompareTo(Accuracy);
        }
    }
}
