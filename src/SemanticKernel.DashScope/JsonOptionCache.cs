using System.Text.Json;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

internal class JsonOptionCache
{
    public static JsonSerializerOptions ReadPermissive { get; } = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };
}