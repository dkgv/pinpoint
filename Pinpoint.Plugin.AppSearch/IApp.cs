namespace Pinpoint.Plugin.AppSearch
{
    public interface IApp
    {
        string Name { get; set; }

        string FilePath { get; set; }

        void Open();

        void OpenDirectory();
    }
}