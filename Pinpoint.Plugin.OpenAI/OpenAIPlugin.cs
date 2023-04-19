using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenAI;
using OpenAI.Chat;
using Pinpoint.Core;
using Pinpoint.Core.Results;

namespace Pinpoint.Plugin.OpenAI;

public class OpenAIPlugin : IPlugin
{
    private const string KeyApiKey = "API Key";

    private const string Description =
        "Run queries through OpenAI's models using their API. Prefix prompts with \"?\". Prompts are submitted after 2 seconds of not typing.\n\nExamples: \"?What is the meaning of life?\", \"?how to invert a binary tree\"";
    private OpenAIClient _client;
    
    public PluginMeta Meta { get; set; } = new("OpenAI", Description, PluginPriority.Highest);

    public PluginStorage Storage { get; set; } = new();

    public TimeSpan DebounceTime => TimeSpan.FromMilliseconds(1250);

    public async Task<bool> TryLoad()
    {
        if (!Storage.UserSettings.Any())
        {
            Storage.UserSettings.Put(KeyApiKey, "");
        }
        else
        {
            var apiKey = Storage.UserSettings.Str(KeyApiKey);
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return true;
            }
            
            _client = new OpenAIClient(apiKey);
        }

        return true;
    }

    public async Task<bool> Activate(Query query)
    {
        return query.Prefix() == "?" && query.RawQuery.Length > 1;
    }

    public async IAsyncEnumerable<AbstractQueryResult> Process(Query query, [EnumeratorCancellation] CancellationToken ct)
    {
        yield return new TitleOpenAIResult();
        
        var userPrompt = query.RawQuery[1..];
        var chatPrompts = new List<ChatPrompt>
        {
            new("user", userPrompt),
        };
        var resp = await _client.ChatEndpoint.GetCompletionAsync(new ChatRequest(chatPrompts), ct);
        if (resp is null)
        {
            yield break;
        }

        var reply = resp.FirstChoice.Message.Content;
        reply = reply.TrimStart('\n');
        
        yield return new OpenAIResult(userPrompt, reply);
    }
}