using System;
using System.Collections.Generic;
using System.Linq;

namespace Pinpoint.Plugin.AppSearch;

public class PathAppProvider : IAppProvider
{
    private List<IApp> _cachedApps = new();
    private string _cachedPath;
    
    public IEnumerable<IApp> Provide()
    {
        var environmentVariable = Environment.GetEnvironmentVariable("PATH");
        if (_cachedPath == environmentVariable)
        {
            return _cachedApps;
        }
        
        var paths = environmentVariable?.Split(';') ?? Array.Empty<string>();
        _cachedApps = new DirectoryAppProvider(paths).Provide().ToList();
        _cachedPath = environmentVariable;
        
        return _cachedApps;
    }
}