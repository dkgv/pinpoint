using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        _cachedPath = environmentVariable;
        
        var task = Task.Run(() => new DirectoryAppProvider(paths).Provide().ToList());
        task.ContinueWith(t =>
        {
            _cachedApps = t.Result;
        });
        
        return _cachedApps;
    }
}