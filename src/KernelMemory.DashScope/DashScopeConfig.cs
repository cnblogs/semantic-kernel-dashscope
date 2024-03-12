namespace Cnblogs.KernelMemory.AI.DashScope;

/// <summary>
/// DashScope Settings.
/// </summary>
public record DashScopeConfig
{
    /// <summary>
    /// Model used for text generation. Chat models can be used too.
    /// </summary>
    public string ChatCompletionModelId { get; set; } = string.Empty;

    /// <summary>
    /// The max number of tokens supported by the text model.
    /// </summary>
    public int TextModelMaxTokenTotal { get; set; } = 6000;

    /// <summary>
    /// Model used to embedding generation.
    /// </summary>
    public string TextEmbeddingModelId { get; set; } = string.Empty;

    /// <summary>
    /// The max number of tokens supported by the embedding model.
    /// Defaults to 2048.
    /// </summary>
    public int EmbeddingModelMaxTokenTotal { get; set; } = 2048;

    /// <summary>
    /// DashScope API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Validates the config.
    /// </summary>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            throw new ArgumentOutOfRangeException(nameof(ApiKey), ApiKey, "Api key cannot be null or empty");
        }

        if (TextModelMaxTokenTotal < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(TextModelMaxTokenTotal),
                TextModelMaxTokenTotal,
                $"{nameof(TextModelMaxTokenTotal)} cannot be less than 1");
        }

        if (EmbeddingModelMaxTokenTotal < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(EmbeddingModelMaxTokenTotal),
                EmbeddingModelMaxTokenTotal,
                $"{nameof(EmbeddingModelMaxTokenTotal)} cannot be less than 1");
        }
    }
}
