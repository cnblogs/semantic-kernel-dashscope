using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Sdcb.DashScope;
using Sdcb.DashScope.TextGeneration;

namespace Connectors.Qwen;

public sealed class QwenChatCompletionService : IChatCompletionService
{
    private readonly DashScopeClient _dashScopeClient;
    private readonly string _modelId;
    private readonly Dictionary<string, object?> _attribues = [];

    public QwenChatCompletionService(
        IOptions<QwenClientOptions> options,
        HttpClient httpClient)
    {
        _dashScopeClient = new(options.Value.ApiKey, httpClient);
        _modelId = options.Value.ModelId;
        _attribues.Add(AIServiceExtensions.ModelIdKey, _modelId);
    }

    public IReadOnlyDictionary<string, object?> Attributes => _attribues;

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var chatMessages = chatHistory
            .Where(x => !string.IsNullOrEmpty(x.Content))
            .Select(x => new ChatMessage(x.Role.ToString(), x.Content!)).
            ToList();

        var response = await _dashScopeClient.TextGeneration.Chat(
            _modelId,
            chatMessages,
            cancellationToken: cancellationToken);

        return [new ChatMessageContent(new AuthorRole(chatMessages.First().Role), response.Output.Text)];
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatMessages = chatHistory
            .Where(x => !string.IsNullOrEmpty(x.Content))
            .Select(x => new ChatMessage(x.Role.ToString(), x.Content!)).
            ToList();

        var responses = _dashScopeClient.TextGeneration.ChatStreamed(
            _modelId,
            chatMessages,
            cancellationToken: cancellationToken);

        await foreach (var response in responses)
        {
            yield return new StreamingChatMessageContent(new AuthorRole(chatMessages.First().Role), response.Output.Text);
        }
    }
}
