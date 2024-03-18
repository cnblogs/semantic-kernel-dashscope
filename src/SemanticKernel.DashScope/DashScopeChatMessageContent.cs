using Cnblogs.DashScope.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

/// <summary>
/// DashScope specialized message content
/// </summary>
public class DashScopeChatMessageContent(
    AuthorRole role,
    string content,
    Dictionary<string, object?>? metadata = null,
    string? name = null,
    List<ToolCall>? toolCalls = null)
    : ChatMessageContent(role, content, metadata: metadata)
{
    /// <summary>
    /// The name of tool if role is tool.
    /// </summary>
    public string? Name { get; } = name;

    /// <summary>
    /// Optional tool calls.
    /// </summary>
    public List<ToolCall>? ToolCalls { get; } = toolCalls;
}
