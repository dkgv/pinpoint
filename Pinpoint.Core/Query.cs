namespace Pinpoint.Core
{
    public sealed class Query
    {
        public Query(string rawQuery)
        {
            RawQuery = rawQuery;
            Parts = rawQuery.Split(' ');
        }

        public string Prefix(int n = 1)
        {
            return RawQuery.Substring(0, n);
        }

        public string Bang()
        {
            return !Prefix().Equals("!") ? string.Empty : Parts[0];
        }

        public bool HasBang => !string.IsNullOrEmpty(Bang());

        public bool IsEmpty => RawQuery.Length == 0;

        public string RawQuery { get; }

        public string[] Parts { get; }
    }
}
