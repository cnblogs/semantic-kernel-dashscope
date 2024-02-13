using Microsoft.SemanticKernel.ChatCompletion;
using Sdcb.DashScope.TextGeneration;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

public static class ChatHistoryExtensions
{
    public static IReadOnlyList<ChatMessage> ToChatMessages(this ChatHistory chatHistory)
    {
        return chatHistory
            .Where(x => !string.IsNullOrEmpty(x.Content))
            .Select(x => new ChatMessage(x.Role.ToString(), x.Content!)).
            ToList();
    }
}