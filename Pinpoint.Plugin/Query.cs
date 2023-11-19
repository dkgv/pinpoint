namespace Pinpoint.Plugin;

public sealed class Query
{
    public Query(string rawQuery)
    {
        Raw = rawQuery;
        Parts = rawQuery.Split(' ');
    }

    public string Prefix(int n = 1) => Raw[..n];

    public bool IsEmpty => Raw.Length == 0;

    public string Raw { get; }

    public string[] Parts { get; }

    public int ResultCount = 0;
}