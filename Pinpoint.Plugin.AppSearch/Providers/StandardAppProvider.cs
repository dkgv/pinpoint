using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Pinpoint.Plugin.AppSearch.Models;

namespace Pinpoint.Plugin.AppSearch.Providers;

public class StandardAppProvider : IAppProvider
{
    private static readonly Guid[] DefaultKnownFolders = {
        KnownFolderId.Desktop,
        KnownFolderId.Programs,
        KnownFolderId.StartMenuAllPrograms,
        KnownFolderId.RoamingAppData,
        KnownFolderId.PublicDesktop,
        KnownFolderId.CommonStartMenu,
    };

    private readonly DirectoryAppProvider _directoryAppProvider;

    public StandardAppProvider()
    {
        _directoryAppProvider = new DirectoryAppProvider(
            DefaultKnownFolders.Select(GetPathForKnownFolder)
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct()
                .ToArray()
        );
    }

    public async Task<IEnumerable<IApp>> Provide() => await _directoryAppProvider.Provide();

    private string GetPathForKnownFolder(Guid guid)
    {
        try
        {
            return SHGetKnownFolderPath(guid, 0);
        }
        catch (FileNotFoundException)
        {
        }
        catch (COMException)
        {
        }
        
        return null;
    }
    
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)]
    private static extern string SHGetKnownFolderPath(
        [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
        uint dwFlags,
        IntPtr hToken = default
    );
}