using Cnblogs.DashScope.Core;
using Cnblogs.SemanticKernel.Connectors.DashScope;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;

// ReSharper disable once CheckNamespace
namespace Microsoft.SemanticKernel;

/// <summary>
/// Extensions for DI.
/// </summary>
public static class DashScopeServiceCollectionExtensions
{
    #region TextEmbedding

    /// <summary>
    /// Adds a DashScope text embedding generation service
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <param name="serviceId">The local identifier of service.</param>
    /// <param name="sectionName">The section name in configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddDashScopeTextEmbeddingGeneration(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "dashScope",
        string? serviceId = null)
    {
        var option = configuration.GetOptions(sectionName);
        return services.AddDashScopeTextEmbeddingGeneration(option.ApiKey, option.TextEmbeddingModelId, serviceId);
    }

    /// <summary>
    /// Adds a DashScope text embedding generation service.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="apiKey">The api key of DashScope.</param>
    /// <param name="modelId">The model id.</param>
    /// <param name="serviceId">A local identifier for the given AI service.</param>
    /// <returns></returns>
    public static IServiceCollection AddDashScopeTextEmbeddingGeneration(
        this IServiceCollection services,
        string apiKey,
        string modelId,
        string? serviceId = null)
    {
        return services.AddKeyedSingleton<ITextEmbeddingGenerationService, DashScopeTextEmbeddingGenerationService>(
            serviceId,
            (_, _) => new DashScopeTextEmbeddingGenerationService(modelId, new DashScopeClient(apiKey)));
    }

    #endregion

    #region ChatCompletion

    /// <summary>
    /// Add DashScope as chat completion service and fetch <see cref="DashScopeOptions"/> from <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <param name="serviceId">The local identifier of service.</param>
    /// <param name="sectionName">The section name in configuration.</param>
    /// <returns></returns>
    public static IServiceCollection AddDashScopeChatCompletion(
        this IServiceCollection services,
        IConfiguration configuration,
        string? serviceId = null,
        string sectionName = "dashScope")
    {
        var option = configuration.GetOptions(sectionName);
        return services.AddDashScopeChatCompletion(option.ApiKey, option.ChatCompletionModelId, serviceId);
    }

    /// <summary>
    /// Add DashScope as chat completion service.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/></param>
    /// <param name="apiKey">The api key for DashScope.</param>
    /// <param name="modelId">The model name.</param>
    /// <param name="serviceId">The local identifier of service.</param>
    /// <returns></returns>
    public static IServiceCollection AddDashScopeChatCompletion(
        this IServiceCollection services,
        string apiKey,
        string modelId,
        string? serviceId = null)
    {
        services.AddKeyedSingleton<ITextGenerationService, DashScopeChatCompletionService>(
            serviceId,
            (sp, _) => new DashScopeChatCompletionService(
                modelId,
                new DashScopeClient(apiKey),
                sp.GetRequiredService<ILogger<DashScopeChatCompletionService>>()));
        return services.AddKeyedSingleton<IChatCompletionService, DashScopeChatCompletionService>(
            serviceId,
            (sp, _) => new DashScopeChatCompletionService(
                modelId,
                new DashScopeClient(apiKey),
                sp.GetRequiredService<ILogger<DashScopeChatCompletionService>>()));
    }

    #endregion

    private static DashScopeOptions GetOptions(this IConfiguration configuration, string sectionName)
    {
        return configuration.GetSection(sectionName).Get<DashScopeOptions>()
               ?? throw new InvalidOperationException(
                   $"Can not resolve {nameof(DashScopeOptions)} from section: {sectionName}");
    }
}
