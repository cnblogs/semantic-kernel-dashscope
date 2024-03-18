using Cnblogs.DashScope.Core;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

internal static class DashScopeMapper
{
    public static List<ChatMessage> ToChatMessages(this ChatHistory history)
    {
        return history.Select(
            x =>
            {
                if (x is DashScopeChatMessageContent d)
                {
                    return new ChatMessage(x.Role.Label, x.Content ?? string.Empty, d.Name, ToolCalls: d.ToolCalls);
                }

                return new ChatMessage(x.Role.Label, x.Content ?? string.Empty);
            }).ToList();
    }

    public static Dictionary<string, object?>? ToMetaData<TOutput, TUsage>(
        this ModelResponse<TOutput, TUsage>? response)
        where TUsage : class
        where TOutput : class
    {
        return response == null
            ? null
            : new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                { "Usage", response.Usage }, { "RequestId", response.RequestId }
            };
    }
}
