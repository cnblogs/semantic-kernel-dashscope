using Cnblogs.SemanticKernel.Connectors.DashScope;
using Microsoft.Extensions.DependencyInjection;
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

}