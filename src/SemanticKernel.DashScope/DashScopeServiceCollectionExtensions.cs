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
        Action<HttpClient>? configureClient = null,
        string configSectionPath = "dashscope")
    {
        Func<IServiceProvider, object?, DashScopeChatCompletionService> factory = (serviceProvider, _) =>
            serviceProvider.GetRequiredService<DashScopeChatCompletionService>();

        if (configureClient == null)
        {
            builder.Services.AddHttpClient<DashScopeChatCompletionService>();
        }
        else
        {
            builder.Services.AddHttpClient<DashScopeChatCompletionService>(configureClient);
        }

        builder.Services.AddOptions<DashScopeClientOptions>().BindConfiguration(configSectionPath);
        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return builder;
    }

    public static IKernelBuilder AddDashScopeChatCompletion<T>(
        this IKernelBuilder builder,
        string? serviceId = null,
        Action<HttpClient>? configureClient = null,
        string configSectionPath = "dashscope") where T : class
    {
        if (!builder.Services.Any(s => s.ServiceType == typeof(IConfiguration)))
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", true)
                .AddUserSecrets<T>()
                .Build();
            builder.Services.TryAddSingleton(config);
        }
        return builder.AddDashScopeChatCompletion(serviceId, configureClient, configSectionPath);
    }
}