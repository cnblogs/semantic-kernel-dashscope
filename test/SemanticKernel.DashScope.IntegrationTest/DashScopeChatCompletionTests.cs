using System.Diagnostics;
using System.Text;
using Microsoft.SemanticKernel;

namespace SemanticKernel.DashScope.IntegrationTest;

public class DashScopeChatCompletionTests
{
    [Fact]
    public async Task ChatCompletion_InvokePromptAsync_WorksCorrectly()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.AddDashScopeChatCompletion<DashScopeChatCompletionTests>();
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
        Trace.WriteLine(result.ToString());
    }

    [Fact]
    public async Task ChatCompletion_InvokePromptStreamingAsync_WorksCorrectly()
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.AddDashScopeChatCompletion<DashScopeChatCompletionTests>();
        var kernel = builder.Build();

        // Act
        var prompt = @"<message role=""user"">博客园是什么网站</message>";
        var result = kernel.InvokePromptStreamingAsync(prompt);

        // Assert
        var sb = new StringBuilder();
        await foreach (var message in result)
        {
            Trace.Write(message);
            sb.Append(message);
        }
        Assert.Contains("博客园", sb.ToString());
    }
}