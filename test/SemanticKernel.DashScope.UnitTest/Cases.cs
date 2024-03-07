using Cnblogs.DashScope.Sdk;
using Cnblogs.SemanticKernel.Connectors.DashScope;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernel.DashScope.UnitTest;

public static class Cases
{
    public const string Prompt = "prompt";
    public const string ModelId = "qwen-max";
    public const string ModelIdAlter = "qwen-plus";
    public const string ApiKey = "api-key";

    public static readonly ChatHistory ChatHistory = new("You are a helpful assistant")
    {
        new ChatMessageContent(AuthorRole.User, "请问 1+1 是多少")
    };

    public static readonly IConfiguration Configuration =
        new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>()
            {
                { "dashScope:apiKey", ApiKey },
                { "dashScope:chatCompletionModelId", ModelId },
                { "dashScope:textEmbeddingModelId", ModelIdAlter }
            }).Build();

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

    public static readonly ModelResponse<TextEmbeddingOutput, TextEmbeddingTokenUsage> TextEmbeddingResponse = new()
    {
        Output = new(
        [
            new(
                0,
                [
                    -0.005039233741296242f, 0.014719783208941139f, 0.005160200817840309f, 0.024575416603162974f,
                    0.04125613978976582f, -0.003979180149475871f
                ])
        ]),
        RequestId = "1773f7b2-2148-9f74-b335-b413e398a116",
        Usage = new(3)
    };

    public static readonly ModelResponse<TextGenerationOutput, TextGenerationTokenUsage> ChatGenerationResponse = new()
    {
        Output = new()
        {
            Choices =
            [
                new()
                {
                    FinishReason = "stop",
                    Message = new(
                        "assistant",
                        "1+1 等于 2。这是最基本的数学加法之一，在十进制计数体系中，任何两个相同的数字相加都等于该数字的二倍。")
                }
            ]
        },
        RequestId = "e764bfe3-c0b7-97a0-ae57-cd99e1580960",
        Usage = new()
        {
            TotalTokens = 47,
            OutputTokens = 39,
            InputTokens = 8
        }
    };
}
