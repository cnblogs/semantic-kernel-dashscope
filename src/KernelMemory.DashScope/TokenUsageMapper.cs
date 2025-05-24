using Cnblogs.DashScope.Core;
using Microsoft.KernelMemory;

namespace Cnblogs.KernelMemory.AI.DashScope;

internal static class TokenUsageMapper
{
    public static TokenUsage? ToKernelMemoryTokenUsage(this TextGenerationTokenUsage? usage, string? modelId)
    {
        if (usage == null)
        {
            return null;
        }

        return new TokenUsage()
        {
            ServiceTokensIn = usage.InputTokens,
            ServiceTokensOut = usage.OutputTokens,
            ModelName = modelId
        };
    }
}
