using Pinpoint.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pinpoint.Plugin.GuidGenerator
{
    public class GuidGeneratorPlugin : AbstractPlugin
    {
        public override PluginManifest Manifest => new PluginManifest("GUID generator plugin");

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var guid = Guid.NewGuid();

            // Return results both with and without hyphens.
            yield return new GuidGeneratorResult(guid.ToString("N"));
            yield return new GuidGeneratorResult(guid.ToString("D"));
        }

        public override Task<bool> ShouldActivate(Query query)
        {
            var shouldActivate = query.Raw.Equals("guid", StringComparison.InvariantCultureIgnoreCase) || query.Raw.Equals("uuid", StringComparison.InvariantCultureIgnoreCase);

            return Task.FromResult(shouldActivate);
        }
    }

    public class GuidGeneratorResult : CopyabableQueryOption
    {
        public string Value { get; }

        public GuidGeneratorResult(string value) : base(value)
        {
            Value = value;
        }
    }
}
