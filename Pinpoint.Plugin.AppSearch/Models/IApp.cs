namespace Pinpoint.Plugin.AppSearch.Models;

public interface IApp
{
    string Name { get; set; }

    string FilePath { get; set; }

    string IconLocation { get; set; }

    void Open();

    void OpenDirectory();
}