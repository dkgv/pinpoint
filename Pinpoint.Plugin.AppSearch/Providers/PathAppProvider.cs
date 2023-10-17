using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pinpoint.Plugin.AppSearch.Models;

namespace Pinpoint.Plugin.AppSearch.Providers;

public class PathAppProvider : IAppProvider
{
    private List<IApp> _cachedApps = new();
    private string _cachedPath;
    
    public async Task<IEnumerable<IApp>> Provide()
    {
        var environmentVariable = Environment.GetEnvironmentVariable("PATH");
        if (_cachedPath == environmentVariable)
        {
            return _cachedApps;
        }

        var paths = environmentVariable?.Split(';') ?? Array.Empty<string>();
        var directoryAppProvider = new DirectoryAppProvider(paths);
        var apps = await directoryAppProvider.Provide();

        _cachedPath = environmentVariable;
        _cachedApps = apps.ToList();

        return _cachedApps;
    }
}