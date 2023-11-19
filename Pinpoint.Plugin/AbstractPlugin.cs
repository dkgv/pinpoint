using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using Newtonsoft.Json;

namespace Pinpoint.Plugin;

public abstract class AbstractPlugin : IComparable<AbstractPlugin>
{
    public AbstractPlugin()
    {
        try
        {
            State ??= new PluginState();
            Storage ??= new PluginStorage();
            State.IsEnabled = true;

            if (File.Exists(FilePath))
            {
                dynamic obj = JsonConvert.DeserializeObject(File.ReadAllText(FilePath));
                State.IsEnabled = obj?.IsEnabled ?? true;

                var storage = JsonConvert.DeserializeObject<PluginStorage>(obj.Storage.ToString());
                if (storage.User != null)
                {
                    Storage.User.Overwrite(storage.User);
                }

                if (storage.Internal != null)
                {
                    Storage.Internal = storage.Internal;
                }
            }
       }
        catch (FileNotFoundException e)
        {
            Debug.WriteLine(Manifest.Name + ": " + e.Message);
        }
        catch (RuntimeBinderException e)
        {
            Debug.WriteLine(Manifest.Name + ": " + e.Message);
        }
        catch (JsonSerializationException e)
        {
            Debug.WriteLine(Manifest.Name + ": " + e.Message);
        }
    }

    public abstract PluginManifest Manifest { get; }

    public virtual PluginStorage Storage { get; }

    public virtual PluginState State { get; }

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
            State.IsEnabled,
            Storage
        };
        var json = JsonConvert.SerializeObject(dump, Formatting.Indented);
        File.WriteAllText(FilePath, json);
    }

    public static readonly string MainDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Path.DirectorySeparatorChar + "Pinpoint" + Path.DirectorySeparatorChar;

    private string FilePath => Path.Combine(MainDirectory, $"{string.Concat(Manifest.Name.Split(Path.GetInvalidFileNameChars()))}.json");
}
