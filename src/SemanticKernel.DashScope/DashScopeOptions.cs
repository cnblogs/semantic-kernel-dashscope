namespace Cnblogs.SemanticKernel.Connectors.DashScope;

/// <summary>
/// Options about DashScopeClient
/// </summary>
public class DashScopeOptions
{
    /// <summary>
    /// The text embedding model id.
    /// </summary>
    public string TextEmbeddingModelId { get; set; } = string.Empty;

    /// <summary>
    /// Default model name for chat completion.
    /// </summary>
    public string ChatCompletionModelId { get; set; } = string.Empty;

    /// <summary>
    /// The DashScope api key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
