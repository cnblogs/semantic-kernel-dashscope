﻿using Cnblogs.DashScope.Core;
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
    int maxTokens = 8192)
    : ITextEmbeddingGenerator
{
    /// <inheritdoc />
    public int CountTokens(string text)
    {
        return tokenizer?.CountTokens(text) ?? text.Length;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetTokens(string text)
    {
        return tokenizer?.GetTokens(text) ?? [text];
    }

    /// <inheritdoc />
    public async Task<Embedding> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = new())
    {
        var result = await dashScopeClient.GetEmbeddingsAsync(
            new ModelRequest<TextEmbeddingInput, ITextEmbeddingParameters>
            {
                Input = new TextEmbeddingInput { Texts = [text] },
                Model = modelId
            },
            cancellationToken);
        return result.Output.Embeddings[0].Embedding;
    }

    /// <inheritdoc />
    public int MaxTokens => maxTokens;
}
