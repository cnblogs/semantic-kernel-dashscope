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
        Func<IServiceProvider, object?, DashScopeChatCompletionService> factory = (serviceProvider, _) =>
            serviceProvider.GetRequiredService<DashScopeChatCompletionService>();

        var optionsBuilder = builder.Services.AddOptions<DashScopeClientOptions>().BindConfiguration(configSectionPath);
        if (configureOptions != null) optionsBuilder.PostConfigure(configureOptions);

        var httpClientBuilder = configureClient == null
            ? builder.Services.AddHttpClient<DashScopeChatCompletionService>()
            : builder.Services.AddHttpClient<DashScopeChatCompletionService>(configureClient);

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return builder;
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
        builder.Services.AddConfiguration<T>();

        void AggConfigureOptions(DashScopeClientOptions options)
        {
            if (!string.IsNullOrEmpty(modelId)) options.ModelId = modelId;
            if (!string.IsNullOrEmpty(apiKey)) options.ApiKey = apiKey;
            configureOptions?.Invoke(options);
        }

        return builder.AddDashScopeChatCompletion(serviceId, AggConfigureOptions, configureClient, configSectionPath);
    }

    public static IKernelBuilder AddDashScopeChatCompletion(
        this IKernelBuilder builder,
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

        builder.Services.AddHttpClient();
        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return builder;
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