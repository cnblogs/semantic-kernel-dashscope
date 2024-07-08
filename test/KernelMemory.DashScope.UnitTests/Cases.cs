using Cnblogs.DashScope.Core;
using Cnblogs.KernelMemory.AI.DashScope;
using Microsoft.KernelMemory.AI;

namespace KernelMemory.DashScope.UnitTests;

public static class Cases
{
    public const string Text = "代码改变世界";
    public static readonly int[] Tokens = [46100, 101933, 99489];
    public const string ModelId = "qwen-max";

    public static readonly TextGenerationOptions TextGenerationOptions = new()
    {
        NucleusSampling = 0.8,
        FrequencyPenalty = 0.1,
        MaxTokens = 1000,
        PresencePenalty = 0,
        ResultsPerPrompt = 1,
        StopSequences = ["你好"],
        TokenSelectionBiases = new() { { 19432, 0.5f } }
    };

    public static readonly TextGenerationParameters TextGenerationParameters = new()
    {
        TopP = 0.8f,
        RepetitionPenalty = 1.1f,
        MaxTokens = 1000,
        EnableSearch = null,
        IncrementalOutput = true,
        ResultFormat = ResultFormats.Text,
        Seed = null,
        Stop = (string[]) ["你好"],
        TopK = null
    };

    public static readonly ModelResponse<TextGenerationOutput, TextGenerationTokenUsage> TextGenerationResponse = new()
    {
        Output = new() { FinishReason = "stop", Text = "1+1 等于 2。这是最基本的数学加法之一，在十进制计数体系中，任何情况下两个一相加的结果都是二。" },
        RequestId = "4ef2ed16-4dc3-9083-a723-fb2e80c84d3b",
        Usage = new()
        {
            InputTokens = 8,
            OutputTokens = 35,
            TotalTokens = 43
        }
    };

    public static readonly float[] Embeddings =
    [
        -0.011274966097430674f,
        0.019980622395909534f,
        0.009520792656014333f,
        0.01861119331524994f,
        0.0346400346498275f,
        -0.0045615030567685115f,
        0.020032791122791806f,
        0.02697123179813376f,
        -0.003495304701112112f,
        0.03883961716385025f,
        0.02131092493140743f
    ];

    public static readonly ModelResponse<TextEmbeddingOutput, TextEmbeddingTokenUsage> TextEmbeddingResponse = new()
    {
        Output = new(
        [
            new TextEmbeddingItem(
                0,
                Embeddings)
        ]),
        RequestId = "4ef2ed16-4dc3-9083-a723-fb2e80c84d3b",
        Usage = new(3)
    };

    public static readonly DashScopeConfig DashScopeConfig = new()
    {
        ApiKey = "apiKey",
        TextEmbeddingModelId = "text-embedding-v2",
        EmbeddingModelMaxTokenTotal = 1000,
        ChatCompletionModelId = "qwen-max",
        TextModelMaxTokenTotal = 1000
    };

    public static readonly DashScopeConfig InvalidConfig = new()
    {
        ApiKey = string.Empty,
        TextEmbeddingModelId = "text-embedding-v2",
        EmbeddingModelMaxTokenTotal = 1000,
        ChatCompletionModelId = "qwen-max",
        TextModelMaxTokenTotal = 1000
    };

    public static readonly Dictionary<string, string?> Configurations = new()
    {
        { "dashScope:apiKey", "apiKey" },
        { "dashScope:chatCompletionModelId", "qwen-max" },
        { "dashScope:textEmbeddingModelId", "text-embedding-v2" }
    };
}
