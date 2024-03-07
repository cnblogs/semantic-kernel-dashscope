using System.Runtime.CompilerServices;
using Cnblogs.DashScope.Sdk;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TextGeneration;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

/// <summary>
/// DashScope chat completion service.
/// </summary>
public sealed class DashScopeChatCompletionService : IChatCompletionService, ITextGenerationService
{
    private readonly IDashScopeClient _dashScopeClient;
    private readonly Dictionary<string, object?> _attributes = new();
    private readonly string _modelId;

    /// <summary>
    /// Creates a new DashScope chat completion service.
    /// </summary>
    /// <param name="modelId"></param>
    /// <param name="dashScopeClient"></param>
    public DashScopeChatCompletionService(string modelId, IDashScopeClient dashScopeClient)
    {
        _dashScopeClient = dashScopeClient;
        _modelId = modelId;
        _attributes.Add(AIServiceExtensions.ModelIdKey, _modelId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var chatMessages = chatHistory.ToChatMessages();
        var chatParameters = DashScopePromptExecutionSettings.FromPromptExecutionSettings(executionSettings);
        chatParameters ??= new DashScopePromptExecutionSettings();
        chatParameters.IncrementalOutput = false;
        chatParameters.ResultFormat = ResultFormats.Message;
        var response = await _dashScopeClient.GetTextCompletionAsync(
            new ModelRequest<TextGenerationInput, ITextGenerationParameters>
            {
                Input = new TextGenerationInput { Messages = chatMessages },
                Model = string.IsNullOrEmpty(chatParameters.ModelId) ? _modelId : chatParameters.ModelId,
                Parameters = chatParameters
            },
            cancellationToken);
        var message = response.Output.Choices![0].Message;
        var chatMessageContent = new ChatMessageContent(
            new AuthorRole(message.Role),
            message.Content,
            metadata: response.ToMetaData());
        return [chatMessageContent];
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var chatMessages = chatHistory.ToChatMessages();
        executionSettings ??= new DashScopePromptExecutionSettings();
        var parameters = DashScopePromptExecutionSettings.FromPromptExecutionSettings(executionSettings);
        parameters.IncrementalOutput = true;
        parameters.ResultFormat = ResultFormats.Message;
        var responses = _dashScopeClient.GetTextCompletionStreamAsync(
            new ModelRequest<TextGenerationInput, ITextGenerationParameters>
            {
                Input = new TextGenerationInput { Messages = chatMessages },
                Model = string.IsNullOrEmpty(parameters.ModelId) ? _modelId : parameters.ModelId,
                Parameters = parameters
            },
            cancellationToken);

        await foreach (var response in responses)
        {
            var message = response.Output.Choices![0].Message;
            yield return new StreamingChatMessageContent(
                new AuthorRole(message.Role),
                message.Content,
                modelId: _modelId,
                metadata: response.ToMetaData());
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => _attributes;

    /// <inheritdoc />
    public async Task<IReadOnlyList<TextContent>> GetTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = new())
    {
        var chatParameters = DashScopePromptExecutionSettings.FromPromptExecutionSettings(executionSettings);
        chatParameters ??= new DashScopePromptExecutionSettings();
        chatParameters.IncrementalOutput = false;
        chatParameters.ResultFormat = ResultFormats.Text;
        var response = await _dashScopeClient.GetTextCompletionAsync(
            new ModelRequest<TextGenerationInput, ITextGenerationParameters>
            {
                Input = new TextGenerationInput { Prompt = prompt },
                Model = string.IsNullOrEmpty(chatParameters.ModelId) ? _modelId : chatParameters.ModelId,
                Parameters = chatParameters
            },
            cancellationToken);
        return [new TextContent(response.Output.Text, _modelId, metadata: response.ToMetaData())];
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<StreamingTextContent> GetStreamingTextContentsAsync(
        string prompt,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        executionSettings ??= new DashScopePromptExecutionSettings();
        var parameters = DashScopePromptExecutionSettings.FromPromptExecutionSettings(executionSettings);
        parameters.IncrementalOutput = true;
        parameters.ResultFormat = ResultFormats.Text;
        var responses = _dashScopeClient.GetTextCompletionStreamAsync(
            new ModelRequest<TextGenerationInput, ITextGenerationParameters>
            {
                Input = new TextGenerationInput { Prompt = prompt },
                Model = string.IsNullOrEmpty(parameters.ModelId) ? _modelId : parameters.ModelId,
                Parameters = parameters
            },
            cancellationToken);

        await foreach (var response in responses)
        {
            yield return new StreamingTextContent(
                response.Output.Text,
                modelId: _modelId,
                metadata: response.ToMetaData());
        }
    }
}
