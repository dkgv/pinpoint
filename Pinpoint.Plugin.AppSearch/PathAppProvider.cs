using System;
using System.Collections.Generic;
using System.Linq;

namespace Pinpoint.Plugin.AppSearch;

public class PathAppProvider : IAppProvider
{
    private List<IApp> _cachedApps = new();
    private string _path;
    
    public IEnumerable<IApp> Provide()
    {
        var environmentVariable = Environment.GetEnvironmentVariable("PATH");
        if (_path == environmentVariable)
        {
            return _cachedApps;
        }
        
        _path = environmentVariable;
        
        var paths = environmentVariable?.Split(';') ?? Array.Empty<string>();
        var apps = new DirectoryAppProvider(paths).Provide();
        _cachedApps = apps.ToList();
        return _cachedApps;
    }
}