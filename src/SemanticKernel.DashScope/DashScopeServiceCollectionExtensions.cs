using Cnblogs.SemanticKernel.Connectors.DashScope;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Microsoft.SemanticKernel;

public static class DashScopeServiceCollectionExtensions
{
    public static IKernelBuilder AddDashScopeChatCompletion(
        this IKernelBuilder builder,
        string? serviceId = null,
        Action<DashScopeClientOptions>? configureOptions = null,
        Action<HttpClient>? configureClient = null,
        string configSectionPath = "dashscope")
    {
        builder.Services.AddDashScopeChatCompletion(serviceId, configureOptions, configureClient, configSectionPath);
        return builder;
    }

    public static IServiceCollection AddDashScopeChatCompletion(
        this IServiceCollection services,
        string? serviceId = null,
        Action<DashScopeClientOptions>? configureOptions = null,
        Action<HttpClient>? configureClient = null,
        string configSectionPath = "dashscope")
    {
        Func<IServiceProvider, object?, DashScopeChatCompletionService> factory = (serviceProvider, _) =>
            serviceProvider.GetRequiredService<DashScopeChatCompletionService>();

        var optionsBuilder = services.AddOptions<DashScopeClientOptions>().BindConfiguration(configSectionPath);
        if (configureOptions != null) optionsBuilder.PostConfigure(configureOptions);

        var httpClientBuilder = configureClient == null
            ? services.AddHttpClient<DashScopeChatCompletionService>()
            : services.AddHttpClient<DashScopeChatCompletionService>(configureClient);

        services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return services;
    }

    public static IKernelBuilder AddDashScopeChatCompletion<T>(
        this IKernelBuilder builder,
        string? modelId = null,
        string? apiKey = null,
        string? serviceId = null,
        Action<DashScopeClientOptions>? configureOptions = null,
        Action<HttpClient>? configureClient = null,
        string configSectionPath = "dashscope") where T : class
    {
        builder.Services.AddDashScopeChatCompletion<T>(modelId, apiKey, serviceId, configureOptions, configureClient);
        return builder;
    }

    public static IServiceCollection AddDashScopeChatCompletion<T>(
        this IServiceCollection services,
        string? modelId = null,
        string? apiKey = null,
        string? serviceId = null,
        Action<DashScopeClientOptions>? configureOptions = null,
        Action<HttpClient>? configureClient = null,
        string configSectionPath = "dashscope") where T : class
    {
        services.AddConfiguration<T>();

        void AggConfigureOptions(DashScopeClientOptions options)
        {
            if (!string.IsNullOrEmpty(modelId)) options.ModelId = modelId;
            if (!string.IsNullOrEmpty(apiKey)) options.ApiKey = apiKey;
            configureOptions?.Invoke(options);
        }

        return services.AddDashScopeChatCompletion(serviceId, AggConfigureOptions, configureClient, configSectionPath);
    }

    public static IKernelBuilder AddDashScopeChatCompletion(
        this IKernelBuilder builder,
        string modelId,
        string apiKey,
        string? serviceId = null,
        Action<HttpClient>? configureClient = null)
    {
        builder.Services.AddDashScopeChatCompletion(modelId, apiKey, serviceId, configureClient);
        return builder;
    }

    public static IServiceCollection AddDashScopeChatCompletion(
        this IServiceCollection services,
        string modelId,
        string apiKey,
        string? serviceId = null,
        Action<HttpClient>? configureClient = null)
    {
        Func<IServiceProvider, object?, DashScopeChatCompletionService> factory = (serviceProvider, _) =>
        {
            var options = new DashScopeClientOptions { ModelId = modelId, ApiKey = apiKey };
            var httpClient = serviceProvider.GetRequiredService<HttpClient>();
            configureClient?.Invoke(httpClient);
            return new DashScopeChatCompletionService(options, httpClient);
        };

        services.AddHttpClient();
        services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return services;
    }

    private static IServiceCollection AddConfiguration<T>(this IServiceCollection services) where T : class
    {
        if (!services.Any(s => s.ServiceType == typeof(IConfiguration)))
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<T>()
                .Build();
            services.TryAddSingleton(config);
        }

        return services;
    }
}