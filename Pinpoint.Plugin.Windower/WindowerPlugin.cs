using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.Windower;

public class WindowerPlugin : AbstractPlugin
{
    public override PluginManifest Manifest { get; } = new("Window Plugin")
    {
        Description = "Easily search and toggle between open app windows.\n\nExamples: \"w <window query>\""
    };

    public async override IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, CancellationToken ct)
    {
        var app = string.Join(" ", query.Parts[1..]);
        var windows = WindowManager.GetWindows();

        foreach (var result in windows
            .Where(w => w.QueryMatch(app))
            .Select(w => new WindowResult(w)))
        {
            yield return result;
        }
    }

    public override Task<bool> ShouldActivate(Query query)
    {
        return Task.FromResult(query.Prefix() == "w" && query.Parts.Length > 1);
    }
}
