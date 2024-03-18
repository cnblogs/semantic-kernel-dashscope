using Cnblogs.DashScope.Core;
using Cnblogs.KernelMemory.AI.DashScope;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.AI;

#pragma warning disable IDE0130 // reduce number of "using" statements
// ReSharper disable once CheckNamespace - reduce number of "using" statements
namespace Microsoft.KernelMemory;

/// <summary>
/// Helper methods for DI.
/// </summary>
public static class DependencyInjector
{
    private const string DefaultTextModel = "qwen-max";
    private const int DefaultTextModelMaxToken = 6000;

    private const string DefaultEmbeddingModel = "text-embedding-v2";
    private const int DefaultEmbeddingModelMaxToken = 2048;

    /// <summary>
    /// Use default DashScope models (qwen-max and text-embedding-v2) and settings for ingestion and retrieval.
    /// </summary>
    /// <param name="builder">Kernel Memory builder</param>
    /// <param name="apiKey">DashScope API Key</param>
    /// <param name="textGenerationTokenizer">Tokenizer used to count tokens used by prompts</param>
    /// <param name="textEmbeddingTokenizer">Tokenizer used to count tokens sent to the embedding generator</param>
    /// <param name="onlyForRetrieval">Whether to use DashScope defaults only for ingestion, and not for retrieval (search and ask API)</param>
    /// <returns>KM builder instance</returns>
    public static IKernelMemoryBuilder WithDashScopeDefaults(
        this IKernelMemoryBuilder builder,
        string apiKey,
        ITextTokenizer? textGenerationTokenizer = null,
        ITextTokenizer? textEmbeddingTokenizer = null,
        bool onlyForRetrieval = false)
    {
        textGenerationTokenizer ??= new QWenTokenizer();
        textEmbeddingTokenizer ??= new LengthTokenizer();

        var config = new DashScopeConfig
        {
            ChatCompletionModelId = DefaultTextModel,
            TextModelMaxTokenTotal = DefaultTextModelMaxToken,
            TextEmbeddingModelId = DefaultEmbeddingModel,
            EmbeddingModelMaxTokenTotal = DefaultEmbeddingModelMaxToken,
            ApiKey = apiKey,
        };
        config.EnsureValid();

        var client = new DashScopeClient(config.ApiKey);
        builder.WithDashScope(config, textEmbeddingTokenizer, textGenerationTokenizer, onlyForRetrieval, client);
        return builder;
    }

    /// <summary>
    /// Use DashScope models for ingestion and retrieval.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelMemoryBuilder"/>.</param>
    /// <param name="configuration">Configuration root.</param>
    /// <param name="sectionName">Section name to bind <see cref="DashScopeConfig"/> from.</param>
    /// <param name="embeddingTokenizer">Tokenizer used to count tokens used by embedding generator.</param>
    /// <param name="textTokenizer">Tokenizer used to count tokens used by prompts</param>
    /// <param name="onlyForRetrieval">Whether to use DashScope only for ingestion, not for retrieval (search and ask API)</param>
    /// <param name="dashScopeClient">The underlying <see cref="IDashScopeClient"/>.</param>
    public static IKernelMemoryBuilder WithDashScope(
        this IKernelMemoryBuilder builder,
        IConfiguration configuration,
        string sectionName = "dashScope",
        ITextTokenizer? embeddingTokenizer = null,
        ITextTokenizer? textTokenizer = null,
        bool onlyForRetrieval = false,
        IDashScopeClient? dashScopeClient = null)
    {
        var config = configuration.GetConfig(sectionName);
        return builder.WithDashScope(config, embeddingTokenizer, textTokenizer, onlyForRetrieval, dashScopeClient);
    }

    /// <summary>
    /// Use DashScope models for ingestion and retrieval.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelMemoryBuilder"/>.</param>
    /// <param name="config">Settings for DashScope.</param>
    /// <param name="embeddingTokenizer">Tokenizer used to count tokens used by embedding generator.</param>
    /// <param name="textTokenizer">Tokenizer used to count tokens used by prompts</param>
    /// <param name="onlyForRetrieval">Whether to use DashScope only for ingestion, not for retrieval (search and ask API)</param>
    /// <param name="dashScopeClient">The underlying <see cref="IDashScopeClient"/>.</param>
    /// <returns></returns>
    public static IKernelMemoryBuilder WithDashScope(
        this IKernelMemoryBuilder builder,
        DashScopeConfig config,
        ITextTokenizer? embeddingTokenizer = null,
        ITextTokenizer? textTokenizer = null,
        bool onlyForRetrieval = false,
        IDashScopeClient? dashScopeClient = null)
    {
        config.EnsureValid();
        embeddingTokenizer ??= new LengthTokenizer();
        textTokenizer ??= new QWenTokenizer();
        dashScopeClient ??= new DashScopeClient(config.ApiKey);
        builder.WithDashScopeTextGeneration(config, textTokenizer, dashScopeClient);
        builder.WithDashScopeTextEmbeddingGeneration(config, embeddingTokenizer, onlyForRetrieval, dashScopeClient);
        return builder;
    }

    /// <summary>
    /// Use DashScope models to generate text.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelMemoryBuilder"/>/</param>
    /// <param name="config">DashScope settings.</param>
    /// <param name="tokenizer">The tokenizer to use.</param>
    /// <param name="dashScopeClient">Underlying <see cref="IDashScopeClient"/>.</param>
    /// <returns></returns>
    public static IKernelMemoryBuilder WithDashScopeTextGeneration(
        this IKernelMemoryBuilder builder,
        DashScopeConfig config,
        ITextTokenizer? tokenizer = null,
        IDashScopeClient? dashScopeClient = null)
    {
        config.EnsureValid();
        tokenizer ??= new QWenTokenizer();
        dashScopeClient ??= new DashScopeClient(config.ApiKey);
        builder.Services.AddDashScopeTextGeneration(config, tokenizer, dashScopeClient);
        return builder;
    }

    /// <summary>
    /// Use DashScope models to generate text embedding.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelMemoryBuilder"/>.</param>
    /// <param name="config">DashScope settings.</param>
    /// <param name="tokenizer">Tokenizer used to count tokens sent to the embedding generator</param>
    /// <param name="onlyForRetrieval">Whether to use DashScope only for ingestion, not for retrieval (search and ask API)</param>
    /// <param name="dashScopeClient">Underlying <see cref="IDashScopeClient"/>.</param>
    /// <returns></returns>
    public static IKernelMemoryBuilder WithDashScopeTextEmbeddingGeneration(
        this IKernelMemoryBuilder builder,
        DashScopeConfig config,
        ITextTokenizer? tokenizer = null,
        bool onlyForRetrieval = false,
        IDashScopeClient? dashScopeClient = null)
    {
        config.EnsureValid();
        tokenizer ??= new LengthTokenizer();
        dashScopeClient ??= new DashScopeClient(config.ApiKey);
        builder.Services.AddDashScopeTextEmbeddingGeneration(config, tokenizer, dashScopeClient);
        if (!onlyForRetrieval)
        {
            builder.AddIngestionEmbeddingGenerator(
                new DashScopeTextEmbeddingGenerator(
                    dashScopeClient,
                    config.TextEmbeddingModelId,
                    tokenizer,
                    config.EmbeddingModelMaxTokenTotal));
        }

        return builder;
    }

    /// <summary>
    /// Implement <see cref="ITextEmbeddingGenerator"/> with DashScope.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">Settings for DashScope.</param>
    /// <param name="tokenizer">The tokenizer to use, defaults to <see cref="LengthTokenizer"/>.</param>
    /// <param name="dashScopeClient">The underlying <see cref="IDashScopeClient"/>.</param>
    /// <returns></returns>
    public static IServiceCollection AddDashScopeTextEmbeddingGeneration(
        this IServiceCollection services,
        DashScopeConfig config,
        ITextTokenizer? tokenizer = null,
        IDashScopeClient? dashScopeClient = null)
    {
        config.EnsureValid();
        tokenizer ??= new LengthTokenizer();

        return services.AddSingleton<ITextEmbeddingGenerator>(
            sp => new DashScopeTextEmbeddingGenerator(
                dashScopeClient ?? sp.GetRequiredService<IDashScopeClient>(),
                config.TextEmbeddingModelId,
                tokenizer,
                config.EmbeddingModelMaxTokenTotal));
    }

    /// <summary>
    /// Implement <see cref="ITextGenerator"/> with DashScope.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="config">Settings for DashScope.</param>
    /// <param name="tokenizer">The tokenizer to use, defaults to <see cref="QWenTokenizer"/>.</param>
    /// <param name="dashScopeClient">The underlying <see cref="IDashScopeClient"/>.</param>
    /// <returns></returns>
    public static IServiceCollection AddDashScopeTextGeneration(
        this IServiceCollection services,
        DashScopeConfig config,
        ITextTokenizer? tokenizer = null,
        IDashScopeClient? dashScopeClient = null)
    {
        config.EnsureValid();
        tokenizer ??= new QWenTokenizer();

        return services.AddSingleton<ITextGenerator>(
            sp => new DashScopeTextGenerator(
                dashScopeClient ?? sp.GetRequiredService<IDashScopeClient>(),
                config.ChatCompletionModelId,
                sp.GetService<ILoggerFactory>(),
                tokenizer,
                config.TextModelMaxTokenTotal));
    }

    private static DashScopeConfig GetConfig(this IConfiguration configuration, string sectionName)
    {
        return configuration.GetSection(sectionName).Get<DashScopeConfig>()
               ?? throw new InvalidOperationException(
                   $"Can not resolve {nameof(DashScopeConfig)} from section: {sectionName}");
    }
}
