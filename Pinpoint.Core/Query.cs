namespace Pinpoint.Core
{
    public class Query
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

        public bool HasBang()
        {
            return !string.IsNullOrEmpty(Bang());
        }

        public string RawQuery { get; set; }

        public string[] Parts { get; set; }
    }
}
