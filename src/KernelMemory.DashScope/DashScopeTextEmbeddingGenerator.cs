using Cnblogs.DashScope.Sdk;
using Cnblogs.DashScope.Sdk.TextEmbedding;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;

namespace Cnblogs.KernelMemory.AI.DashScope;

/// <summary>
/// DashScope text embedding generator implementation.
/// </summary>
/// <param name="dashScopeClient">The <see cref="IDashScopeClient"/>.</param>
/// <param name="modelId">The model id to use.</param>
/// <param name="tokenizer">The tokenizer to use.</param>
/// <param name="maxTokens">Maximum token limit.</param>
public class DashScopeTextEmbeddingGenerator(
    IDashScopeClient dashScopeClient,
    string modelId,
    ITextTokenizer? tokenizer = null,
    int maxTokens = 2048)
    : ITextEmbeddingGenerator
{
    /// <inheritdoc />
    public int CountTokens(string text)
    {
        return tokenizer?.CountTokens(text) ?? text.Length;
    }

    /// <inheritdoc />
    public async Task<Embedding> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = new())
    {
        var result = await dashScopeClient.GetTextEmbeddingsAsync(modelId, [text], null, cancellationToken);
        return result.Output.Embeddings[0].Embedding;
    }

    /// <inheritdoc />
    public int MaxTokens => maxTokens;
}
