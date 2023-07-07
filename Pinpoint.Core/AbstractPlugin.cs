using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;
using Pinpoint.Core.Results;

namespace Pinpoint.Core;

public abstract class AbstractPlugin : IComparable<AbstractPlugin>
{
    public AbstractPlugin()
    {
        try
        {
            dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(FilePath));
            State = JsonConvert.DeserializeObject<PluginState>(obj.State.ToString());
            Storage = JsonConvert.DeserializeObject<PluginStorage>(obj.Storage.ToString());
        }
        catch (FileNotFoundException e)
        {
            Debug.WriteLine(Manifest.Name + ": " + e);
        }
        catch (RuntimeBinderException e)
        {
            Debug.WriteLine(Manifest.Name + ": " + e.Message);
        }
    }

    public abstract PluginManifest Manifest { get; }

    public virtual PluginStorage Storage { get; } = new();

    public virtual PluginState State { get; } = new();

    public virtual Task<bool> Initialize() => Task.FromResult(true);

    public abstract Task<bool> ShouldActivate(Query query);

    public abstract IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, CancellationToken ct);

    int IComparable<AbstractPlugin>.CompareTo(AbstractPlugin other)
    {
        return other.Manifest.Priority.CompareTo(Manifest.Priority);
    }

    public void Save()
    {
        dynamic dump = new
        {
            State = State,
            Storage = Storage
        };
        var json = JsonConvert.SerializeObject(dump, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }


    private string FilePath => Path.Combine(AppConstants.MainDirectory, $"{string.Concat(Manifest.Name.Split(Path.GetInvalidFileNameChars()))}.json");
}
