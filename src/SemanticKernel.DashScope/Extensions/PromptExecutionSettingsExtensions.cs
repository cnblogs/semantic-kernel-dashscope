using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Sdcb.DashScope.TextGeneration;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

public static class PromptExecutionSettingsExtensions
{
    private readonly static JsonSerializerOptions JsonSerializerOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public static ChatParameters? ToChatParameters(this PromptExecutionSettings executionSettings)
    {
        ChatParameters? chatParameters = null;

        if (executionSettings?.ExtensionData?.Count > 0)
        {
            var json = JsonSerializer.Serialize(executionSettings.ExtensionData);
            chatParameters = JsonSerializer.Deserialize<ChatParameters>(json, JsonSerializerOptions);
        }

        return chatParameters;
    }
}