using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.PasswordGenerator
{
    public class PasswordGeneratorPlugin : AbstractPlugin
    {
        private const string ActivateString = "password";
        private readonly char[] _characters =
            "abcdefgijklmnopqrstuvxyzABCDEFGHIJKLMNOPQRSTUVXYZ1234567890+#¤%&/()=@£$€{[]}?<>".ToCharArray();

        public override PluginManifest Manifest { get; } = new PluginManifest("Password Generator", PluginPriority.High)
        {
            Description = "Generate and copy + insert passwords of variable length.\n\nExamples: \"password <length>\", \"password 14\""
        };

        public override Task<bool> ShouldActivate(Query query)
        {
            var shouldActivate = query.Parts.Length == 2 && query.Parts[0] == ActivateString && int.TryParse(query.Parts[1], out _);
            return Task.FromResult(shouldActivate);
        }

        public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
        {
            var couldParseLength = int.TryParse(query.Parts[1], out var length);
            if (!couldParseLength || length < 1 || length > 100)
            {
                yield return new PasswordErrorResult();
                yield break;
            }

            var password = GeneratePassword(length);
            yield return new PasswordResult(password);
        }

        private string GeneratePassword(int length)
        {
            var buffer = new byte[length];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(buffer);

            var chars = new char[length];

            for (int i = 0; i < buffer.Length; i++)
            {
                int byteAsInt = buffer[i];
                var index = byteAsInt % _characters.Length;
                chars[i] = _characters[index];
            }

            return new string(chars);
        }
    }
}
