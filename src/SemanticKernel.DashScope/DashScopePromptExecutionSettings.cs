using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Cnblogs.DashScope.Sdk;
using Microsoft.SemanticKernel;

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
    public int? MaxTokens { get; set; }

    /// <inheritdoc />
    public float? RepetitionPenalty { get; set; }

    /// <inheritdoc />
    public float? Temperature { get; set; }

    /// <inheritdoc />
    public TextGenerationStop? Stop { get; set; }

    /// <inheritdoc />
    public bool? EnableSearch { get; set; }

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
