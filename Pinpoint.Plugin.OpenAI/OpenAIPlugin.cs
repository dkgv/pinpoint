﻿using System.Runtime.CompilerServices;
using OpenAI;
using OpenAI.Chat;

namespace Pinpoint.Plugin.OpenAI;

public class OpenAIPlugin : AbstractPlugin
{
    private const string KeyApiKey = "API Key";

    private OpenAIClient _client;

    public override PluginManifest Manifest { get; } = new("OpenAI", PluginPriority.High)
    {
        Description = "Run queries through OpenAI's models using their API. Prefix prompts with \"?\". Prompts are submitted after 2 seconds of not typing.\n\nExamples: \"?What is the meaning of life?\", \"?how to invert a binary tree\""
    };

    public override PluginState State { get; } = new()
    {
        DebounceTime = TimeSpan.FromMilliseconds(1250)
    };

    public override PluginStorage Storage { get; } = new()
    {
        User = new UserSettings{
            {KeyApiKey, ""}
        }
    };

    public override async Task<bool> Initialize()
    {
        var apiKey = Storage.User.Str(KeyApiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return true;
        }

        _client = new OpenAIClient(apiKey);

        return true;
    }

    public override async Task<bool> ShouldActivate(Query query)
    {
        return query.Prefix() == "?" && query.Raw.Length > 1;
    }

    public override async IAsyncEnumerable<AbstractQueryResult> ProcessQuery(Query query, [EnumeratorCancellation] CancellationToken ct)
    {
        yield return new TitleOpenAIResult();

        var userPrompt = query.Raw[1..];
        var chatPrompts = new List<Message>
        {
            new(Role.User, userPrompt),
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