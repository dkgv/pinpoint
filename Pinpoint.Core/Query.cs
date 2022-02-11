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

        public bool IsEmpty => RawQuery.Length == 0;

        public string RawQuery { get; }

        public string[] Parts { get; }

        public int ResultCount = 0;
    }
}
