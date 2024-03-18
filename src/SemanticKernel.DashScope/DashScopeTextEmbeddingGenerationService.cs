using Cnblogs.DashScope.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Services;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

/// <summary>
/// DashScope text embedding service.
/// </summary>
public class DashScopeTextEmbeddingGenerationService : ITextEmbeddingGenerationService
{
    private readonly IDashScopeClient _client;
    private readonly string _modelId;
    private readonly Dictionary<string, object?> _attributes = new();

    /// <summary>
    /// Create an instance of the DashScope text embedding connector.
    /// </summary>
    /// <param name="modelId">The model name.</param>
    /// <param name="client">The underlying <see cref="IDashScopeClient"/>.</param>
    public DashScopeTextEmbeddingGenerationService(string modelId, IDashScopeClient client)
    {
        _client = client;
        _modelId = modelId;
        _attributes.Add(AIServiceExtensions.ModelIdKey, modelId);
    }

    /// <inheritdoc />
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IList<string> data,
        Kernel? kernel = null,
        CancellationToken cancellationToken = new())
    {
        var result = new List<ReadOnlyMemory<float>>(data.Count);
        var embeddings = await _client.GetEmbeddingsAsync(
            new ModelRequest<TextEmbeddingInput, ITextEmbeddingParameters>
            {
                Model = _modelId, Input = new TextEmbeddingInput { Texts = data }
            },
            cancellationToken);
        if (embeddings.Output.Embeddings.Count != data.Count)
        {
            throw new KernelException(
                $"Expected {data.Count} text embedding(s), but received {embeddings.Output.Embeddings.Count}");
        }

        result.AddRange(embeddings.Output.Embeddings.Select(t => new ReadOnlyMemory<float>(t.Embedding)));

        return result;
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object?> Attributes => _attributes;
}
