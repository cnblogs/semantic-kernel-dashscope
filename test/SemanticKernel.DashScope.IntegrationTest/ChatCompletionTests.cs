using System.Diagnostics;
using System.Text;
using Microsoft.SemanticKernel;
using Sdcb.DashScope.TextGeneration;

namespace SemanticKernel.DashScope.IntegrationTest;

public class ChatCompletionTests
{
    [Fact]
    public async Task ChatCompletion_InvokePromptAsync_WorksCorrectly()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddDashScopeChatCompletion<ChatCompletionTests>();
        var kernel = builder.Build();

        var prompt = @"<message role=""user"">博客园是什么网站</message>";
        PromptExecutionSettings settings = new()
        {
            ExtensionData = new Dictionary<string, object>()
            {
                { "temperature", "0.8" }
            }
        };
        KernelArguments kernelArguments = new(settings);

        // Act
        var result = await kernel.InvokePromptAsync(prompt, kernelArguments);

        // Assert
        Assert.Contains("博客园", result.ToString());
        Assert.Equal(4, GetUsage(result.Metadata)?.InputTokens);
        Trace.WriteLine(result.ToString());
    }

    [Fact]
    public async Task ChatCompletion_InvokePromptStreamingAsync_WorksCorrectly()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddDashScopeChatCompletion<ChatCompletionTests>();
        var kernel = builder.Build();

        // Act
        var prompt = @"<message role=""user"">博客园是什么网站</message>";
        var result = kernel.InvokePromptStreamingAsync(prompt);

        // Assert
        var sb = new StringBuilder();
        await foreach (var content in result)
        {
            Trace.Write(content);
            sb.Append(content);
            Assert.Equal(4, GetUsage(content.Metadata)?.InputTokens);
        }
        Assert.Contains("博客园", sb.ToString());

    }

    private static ChatTokenUsage? GetUsage(IReadOnlyDictionary<string, object?>? metadata)
    {
        return metadata?.TryGetValue("Usage", out var value) == true &&
            value is ChatTokenUsage usage
            ? usage
            : null;
    }
}