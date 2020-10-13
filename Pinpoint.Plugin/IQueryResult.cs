namespace Pinpoint.Plugin
{
    public interface IQueryResult
    {
        public string Title { get; }

        public string Subtitle { get; }

        public object Instance { get; }

        public void OnSelect();
    }
}
