using System;
using System.Collections.Generic;
using System.IO;
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

        bool ModifiableSettings => false;

        bool IsLoaded => true;
        
        TimeSpan DebounceTime => TimeSpan.Zero;

        public Task<bool> TryLoad() => Task.FromResult(true);

        public Task<bool> Activate(Query query);

        public IAsyncEnumerable<AbstractQueryResult> Process(Query query, CancellationToken ct);

        int IComparable<IPlugin>.CompareTo(IPlugin other)
        {
            return other.Meta.Priority.CompareTo(Meta.Priority);
        }

        private string FilePath => Path.Combine(AppConstants.MainDirectory, $"{string.Concat(Meta.Name.Split(Path.GetInvalidFileNameChars()))}.json");

        public async Task Save()
        {
            var json = JsonConvert.SerializeObject(new PluginState(Meta, Storage));
            await File.WriteAllTextAsync(FilePath, json);
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
