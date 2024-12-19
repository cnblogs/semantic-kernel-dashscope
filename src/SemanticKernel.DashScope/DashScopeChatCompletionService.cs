using System.Runtime.CompilerServices;
using System.Text.Json;
using Cnblogs.DashScope.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new DashScope chat completion service.
    /// </summary>
    /// <param name="modelId"></param>
    /// <param name="dashScopeClient"></param>
    /// <param name="loggerFactory"></param>
    public DashScopeChatCompletionService(
        string modelId,
        IDashScopeClient dashScopeClient,
        ILoggerFactory? loggerFactory = null)
    {
        _dashScopeClient = dashScopeClient;
        _modelId = modelId;
        _logger = loggerFactory != null
            ? loggerFactory.CreateLogger<DashScopeChatCompletionService>()
            : NullLogger.Instance;
        _attributes.Add(AIServiceExtensions.ModelIdKey, _modelId);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
        ChatHistory chat,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var chatParameters = DashScopePromptExecutionSettings.FromPromptExecutionSettings(executionSettings);
        chatParameters ??= new DashScopePromptExecutionSettings();
        chatParameters.IncrementalOutput = false;
        chatParameters.ResultFormat = ResultFormats.Message;
        chatParameters.ToolCallBehavior?.ConfigureOptions(kernel, chatParameters);

        var autoInvoke = kernel is not null && chatParameters.ToolCallBehavior?.MaximumAutoInvokeAttempts > 0;
        for (var it = 1; ; it++)
        {
            var chatParametersTools = chatParameters.Tools?.ToList();
            var response = await _dashScopeClient.GetTextCompletionAsync(
                new ModelRequest<TextGenerationInput, ITextGenerationParameters>
                {
                    Input = new TextGenerationInput { Messages = chat.ToChatMessages() },
                    Model = string.IsNullOrEmpty(chatParameters.ModelId) ? _modelId : chatParameters.ModelId,
                    Parameters = chatParameters
                },
                cancellationToken);
            CaptureTokenUsage(response.Usage);
            EnsureChoiceExists(response.Output.Choices);
            var message = response.Output.Choices![0].Message;
            var chatMessageContent = new DashScopeChatMessageContent(
                new AuthorRole(message.Role),
                message.Content,
                name: null,
                toolCalls: message.ToolCalls,
                metadata: response.ToMetaData());
            if (autoInvoke == false || message.ToolCalls is null)
            {
                // no needs to invoke tool
                return [chatMessageContent];
            }

            LogToolCalls(message.ToolCalls);
            chat.Add(chatMessageContent);

            foreach (var call in message.ToolCalls)
            {
                if (call.Type is not ToolTypes.Function)
                {
                    AddResponseMessage(chat, null, "Error: Tool call was not a function call.", call.Id);
                    continue;
                }

                // ensure not calling function that was not included in request list.
                if (chatParametersTools?.Any(
                        x => string.Equals(x.Function?.Name, call.Function.Name, StringComparison.OrdinalIgnoreCase))
                    != true)
                {
                    AddResponseMessage(
                        chat,
                        null,
                        "Error: Function call requests for a function that wasn't defined.",
                        call.Id);
                    continue;
                }

                object? callResult;
                try
                {
                    if (kernel!.Plugins.TryGetKernelFunctionAndArguments(
                            call.Function,
                            out var kernelFunction,
                            out var kernelArguments)
                        == false)
                    {
                        AddResponseMessage(chat, null, "Error: Requested function could not be found.", call.Id);
                        continue;
                    }

                    var functionResult = await kernelFunction.InvokeAsync(kernel, kernelArguments, cancellationToken);
                    callResult = functionResult.GetValue<object>() ?? string.Empty;
                }
                catch (JsonException)
                {
                    AddResponseMessage(chat, null, "Error: Function call arguments were invalid JSON.", call.Id);
                    continue;
                }
                catch (Exception)
                {
                    AddResponseMessage(chat, null, "Error: Exception while invoking function. {e.Message}", call.Id);
                    continue;
                }

                var stringResult = ProcessFunctionResult(callResult, chatParameters.ToolCallBehavior);
                AddResponseMessage(chat, stringResult, null, call.Id);
            }

            chatParameters.Tools = [];
            chatParameters.ToolCallBehavior?.ConfigureOptions(kernel, chatParameters);
            if (it >= chatParameters.ToolCallBehavior!.MaximumAutoInvokeAttempts)
            {
                autoInvoke = false;
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(
                        "Maximum auto-invoke ({MaximumAutoInvoke}) reached",
                        chatParameters.ToolCallBehavior!.MaximumAutoInvokeAttempts);
                }
            }
        }
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
        parameters.ToolCallBehavior?.ConfigureOptions(kernel, parameters);
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
                modelId: string.IsNullOrEmpty(parameters.ModelId) ? _modelId : parameters.ModelId,
                metadata: response.ToMetaData());
        }
    }

    private void CaptureTokenUsage(TextGenerationTokenUsage? usage)
    {
        if (usage is null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Usage info is not available");
            }

            return;
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Input tokens: {InputTokens}. Output tokens: {CompletionTokens}. Total tokens: {TotalTokens}",
                usage.InputTokens,
                usage.OutputTokens,
                usage.TotalTokens);
        }
    }

    private void LogToolCalls(IReadOnlyCollection<ToolCall>? calls)
    {
        if (calls is null)
        {
            return;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Tool requests: {Requests}", calls.Count);
        }

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Function call requests: {Requests}",
                string.Join(", ", calls.Select(ftc => $"{ftc.Function.Name}({ftc.Function.Arguments})")));
        }
    }

    private void AddResponseMessage(ChatHistory chat, string? result, string? errorMessage, string? toolId)
    {
        // Log any error
        if (errorMessage is not null && _logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Failed to handle tool request ({ToolId}). {Error}", toolId, errorMessage);
        }

        // Add the tool response message to both the chat options and to the chat history.
        result ??= errorMessage ?? string.Empty;
        chat.Add(new DashScopeChatMessageContent(AuthorRole.Tool, result, name: toolId));
    }

    private static void EnsureChoiceExists(List<TextGenerationChoice>? choices)
    {
        if (choices is null || choices.Count == 0)
        {
            throw new KernelException("No choice was returned from model");
        }
    }

    private static string ProcessFunctionResult(object functionResult, ToolCallBehavior? toolCallBehavior)
    {
        if (functionResult is string stringResult)
        {
            return stringResult;
        }

        // This is an optimization to use ChatMessageContent content directly
        // without unnecessary serialization of the whole message content class.
        if (functionResult is ChatMessageContent chatMessageContent)
        {
            return chatMessageContent.ToString();
        }

        // For polymorphic serialization of unknown in advance child classes of the KernelContent class,
        // a corresponding JsonTypeInfoResolver should be provided via the JsonSerializerOptions.TypeInfoResolver property.
        // For more details about the polymorphic serialization, see the article at:
        // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-8-0
        return JsonSerializer.Serialize(functionResult, toolCallBehavior?.ToolCallResultSerializerOptions);
    }
}
