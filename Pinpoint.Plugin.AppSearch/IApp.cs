namespace Pinpoint.Plugin.AppSearch
{
    public interface IApp
    {
        string Name { get; set; }

        string FilePath { get; set; }

        string IconLocation { get; set; }

        void Open();

        void OpenDirectory();
    }
}