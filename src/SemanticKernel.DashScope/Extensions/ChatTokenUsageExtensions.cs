using Sdcb.DashScope.TextGeneration;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

public static class ChatTokenUsageExtensions
{
    public static IReadOnlyDictionary<string, object?>? ToMetadata(this ChatTokenUsage? usage)
    {
        return usage is null
            ? null :
            new Dictionary<string, object?>()
            {
                { "Usage",  usage }
            };
    }
}