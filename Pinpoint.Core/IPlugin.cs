using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pinpoint.Core.Results;

namespace Pinpoint.Core
{
    public interface IPlugin : IComparable<IPlugin>
    {
        public PluginMeta Meta { get; set; }

        public PluginStorage Storage { get; set; }

        string ModifiableSettings => "False";

        bool IsLoaded => true;

        public Task<bool> TryLoad() => Task.FromResult(true);

        public void Unload()
        {
        }

        public Task<bool> Activate(Query query);

        public IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct);

        int IComparable<IPlugin>.CompareTo(IPlugin other)
        {
            return other.Meta.Priority.CompareTo(Meta.Priority);
        }

        private string FilePath => Path.Combine(AppConstants.MainDirectory, $"{string.Concat(Meta.Name.Split(Path.GetInvalidFileNameChars()))}.json");

        public void Save()
        {
            var json = JsonConvert.SerializeObject(new PluginState(Meta, Storage));
            File.WriteAllText(FilePath, json);
        }

        public void Restore()
        {
            if (!File.Exists(FilePath))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                if (string.IsNullOrEmpty(json))
                {
                    return;
                }

                var (meta, storage) = JsonConvert.DeserializeObject<PluginState>(json);
                Storage = storage;
                Meta = meta;
            }
            catch
            {
                // ignored
            }
        }

        private record PluginState(PluginMeta Meta, PluginStorage Storage);
    }
}
