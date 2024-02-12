using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cnblogs.SemanticKernel.DashScope;

public static class DashScopeServiceCollectionExtensions
{
    public static IKernelBuilder AddDashScopeChatCompletion(
        this IKernelBuilder builder,
        string? serviceId = null,
        Action<HttpClient>? configureClient = null)
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

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);
        return builder;
    }

}