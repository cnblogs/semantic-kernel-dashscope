using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace SemanticKernel.DashScope.IntegrationTest;

public class DashScopeChatCompletionServiceTests
{
    [Fact]
    public async Task ChatCompletionWorksCorrectlyAsync()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var builder = Kernel.CreateBuilder();
        builder.Services.AddSingleton(config);
        builder.AddDashScopeChatCompletion();
        var kernel = builder.Build();

        var prompt = @"<message role=""user"">博客园是什么网站</message>";
        var summarize = kernel.CreateFunctionFromPrompt(prompt);
        var result = kernel.InvokeStreamingAsync(summarize);

        Console.OutputEncoding = System.Text.Encoding.UTF8;
        await foreach (var message in result)
        {
            //Assert.Contains("博客园", message.ToString());
            Console.WriteLine(message.ToString());
        }
    }
}