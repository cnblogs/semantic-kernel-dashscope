using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Sdcb.DashScope;
using Sdcb.DashScope.TextGeneration;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

public sealed class DashScopeChatCompletionService : IChatCompletionService
{
    private readonly DashScopeClient _dashScopeClient;
    private readonly string _modelId;
    private readonly Dictionary<string, object?> _attribues = [];

    public DashScopeChatCompletionService(
        IOptions<DashScopeClientOptions> options,
        HttpClient httpClient)
    {
        _dashScopeClient = new(options.Value.ApiKey, httpClient);
        _modelId = options.Value.ModelId;
        _attribues.Add(AIServiceExtensions.ModelIdKey, _modelId);
    }

    public IReadOnlyDictionary<string, object?> Attributes => _attribues;

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var chatMessages = chatHistory.ToChatMessages();
        var chatParameters = executionSettings?.ToChatParameters();
        var response = await _dashScopeClient.TextGeneration.Chat(_modelId, chatMessages, chatParameters, cancellationToken);
        var chatMessageContent = new ChatMessageContent(
            new AuthorRole(chatMessages[0].Role),
            response.Output.Text,
            metadata: response.Usage.ToMetadata());
        return [chatMessageContent];
    }

    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatMessages = chatHistory.ToChatMessages();
        var chatParameters = executionSettings?.ToChatParameters() ?? new ChatParameters();
        chatParameters.IncrementalOutput = true;

        var responses = _dashScopeClient.TextGeneration.ChatStreamed(_modelId, chatMessages, chatParameters, cancellationToken);
        await foreach (var response in responses)
        {
            yield return new StreamingChatMessageContent(
                new AuthorRole(chatMessages[0].Role),
                response.Output.Text,
                metadata: response.Usage.ToMetadata());
        }
    }
}
