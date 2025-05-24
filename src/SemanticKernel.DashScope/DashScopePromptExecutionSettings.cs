using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Cnblogs.DashScope.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

/// <summary>
/// Settings for DashScope prompt execution.
/// </summary>
public class DashScopePromptExecutionSettings : PromptExecutionSettings, ITextGenerationParameters
{
    /// <inheritdoc />
    public bool? IncrementalOutput { get; set; }

    /// <inheritdoc />
    public ulong? Seed { get; set; }

    /// <inheritdoc />
    public float? TopP { get; set; }

    /// <inheritdoc />
    public int? TopK { get; set; }

    /// <inheritdoc />
    public string? ResultFormat { get; set; }

    /// <inheritdoc />
    public DashScopeResponseFormat? ResponseFormat { get; }

    /// <inheritdoc />
    public int? MaxTokens { get; set; }

    /// <inheritdoc />
    public float? RepetitionPenalty { get; set; }

    /// <inheritdoc />
    public float? PresencePenalty { get; }

    /// <inheritdoc />
    public float? Temperature { get; set; }

    /// <inheritdoc />
    public TextGenerationStop? Stop { get; set; }

    /// <inheritdoc />
    public bool? EnableSearch { get; set; }

    /// <inheritdoc />
    public ToolChoice? ToolChoice { get; }

    /// <inheritdoc />
    public bool? ParallelToolCalls { get; set; }

    /// <inheritdoc />
    public IEnumerable<ToolDefinition>? Tools { get; set; }

    /// <summary>
    /// Gets or sets the behavior for how tool calls are handled.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>To disable all tool calling, set the property to null (the default).</item>
    /// <item>
    /// To allow the model to request one of any number of functions, set the property to an
    /// instance returned from <see cref="ToolCallBehavior.EnableFunctions"/>, called with
    /// a list of the functions available.
    /// </item>
    /// <item>
    /// To allow the model to request one of any of the functions in the supplied <see cref="Kernel"/>,
    /// set the property to <see cref="ToolCallBehavior.EnableKernelFunctions"/> if the client should simply
    /// send the information about the functions and not handle the response in any special manner, or
    /// <see cref="ToolCallBehavior.AutoInvokeKernelFunctions"/> if the client should attempt to automatically
    /// invoke the function and send the result back to the service.
    /// </item>
    /// </list>
    /// For all options where an instance is provided, auto-invoke behavior may be selected. If the service
    /// sends a request for a function call, if auto-invoke has been requested, the client will attempt to
    /// resolve that function from the functions available in the <see cref="Kernel"/>, and if found, rather
    /// than returning the response back to the caller, it will handle the request automatically, invoking
    /// the function, and sending back the result. The intermediate messages will be retained in the
    /// <see cref="ChatHistory"/> if an instance was provided.
    /// </remarks>
    public ToolCallBehavior? ToolCallBehavior { get; set; }

    [return: NotNullIfNotNull(nameof(settings))]
    internal static DashScopePromptExecutionSettings? FromPromptExecutionSettings(PromptExecutionSettings? settings)
    {
        if (settings is null)
        {
            return null;
        }

        if (settings is DashScopePromptExecutionSettings dashScopePromptExecutionSettings)
        {
            return dashScopePromptExecutionSettings;
        }

        var json = JsonSerializer.Serialize(settings);
        var response =
            JsonSerializer.Deserialize<DashScopePromptExecutionSettings>(json, JsonOptionCache.ReadPermissive);
        if (response is not null)
        {
            return response;
        }

        throw new ArgumentException(
            $"The input execution setting can not be converted to {nameof(DashScopePromptExecutionSettings)}",
            nameof(settings));
    }
}
