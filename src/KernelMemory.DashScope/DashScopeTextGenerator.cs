using System.Runtime.CompilerServices;
using Cnblogs.DashScope.Core;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.Diagnostics;

namespace Cnblogs.KernelMemory.AI.DashScope;

/// <summary>
/// Text generator using DashScope.
/// </summary>
/// <param name="dashScopeClient">The <see cref="IDashScopeClient"/>.</param>
/// <param name="modelId">Model name.</param>
/// <param name="loggerFactory">Logger factory to use.</param>
/// <param name="tokenizer">Tokenizer to use.</param>
/// <param name="maxToken">Maximum token count.</param>
public class DashScopeTextGenerator(
    IDashScopeClient dashScopeClient,
    string modelId,
    ILoggerFactory? loggerFactory = null,
    ITextTokenizer? tokenizer = null,
    int maxToken = 6000) : ITextGenerator
{
    private readonly ILogger<DashScopeTextGenerator> _logger = loggerFactory?.CreateLogger<DashScopeTextGenerator>()
                                                                ?? DefaultLogger<DashScopeTextGenerator>.Instance;

    /// <inheritdoc />
    public int CountTokens(string text)
    {
        return tokenizer?.CountTokens(text) ?? QWenTokenizer.CountTokensStatic(text);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> GenerateTextAsync(
        string prompt,
        TextGenerationOptions options,
        [EnumeratorCancellation] CancellationToken cancellationToken = new())
    {
        var parameters = new TextGenerationParameters
        {
            TopP = options.NucleusSampling == 0 ? null : (float)options.NucleusSampling,
            Temperature = options.Temperature == 0 ? null : (float)options.Temperature,
            RepetitionPenalty =
                options.FrequencyPenalty == 0 ? null : ((float)options.FrequencyPenalty + 1), // dashScope's default value is 1.0, kernel memory is 0.0
            MaxTokens = options.MaxTokens == 0 ? null : options.MaxTokens,
            Stop = options.StopSequences.ToArray(),
            IncrementalOutput = true,
            ResultFormat = ResultFormats.Text
        };

        if (options.TokenSelectionBiases.Count != 0)
        {
            _logger.LogWarning("TokenSelectionBiases is not supported by DashScope and will be ignored");
        }

        var request = new ModelRequest<TextGenerationInput, ITextGenerationParameters>
        {
            Model = modelId,
            Input = new TextGenerationInput { Prompt = prompt },
            Parameters = parameters
        };
        var tokens = dashScopeClient.GetTextCompletionStreamAsync(request, cancellationToken);
        await foreach (var token in tokens)
        {
            yield return token.Output.Text!;
        }
    }

    /// <inheritdoc />
    public int MaxTokenTotal => maxToken;
}
