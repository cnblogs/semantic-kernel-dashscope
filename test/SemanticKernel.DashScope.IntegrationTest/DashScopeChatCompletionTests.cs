using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace SemanticKernel.DashScope.IntegrationTest;

public class DashScopeChatCompletionTests
{
    [Fact]
    public async Task ChatCompletion_InvokePromptAsync_WorksCorrectly()
    {
        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(GetConfiguration());
        builder.AddDashScopeChatCompletion();
        var kernel = builder.Build();

        var prompt = @"<message role=""user"">博客园是什么网站</message>";
        var result = await kernel.InvokePromptAsync(prompt);
        Assert.Contains("博客园", result.ToString());
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine(result);
    }

    private IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<DashScopeChatCompletionTests>()
            .Build();
    }
}