# SemanticKernel.Connectors.DashScope
Semantic Kernel Connector to DashScope

## Get started
Add the NuGet package to your project.
```shell
dotnet add package Cnblogs.SemanticKernel.Connectors.DashScope
```

```cs
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();
builder.Service.AddDashScopeChatCompletion("your-api-key", "qwen-max");
var kernel = builder.Build();

var prompt = @"<message role=""user"">Tell me about the Cnblogs</message>";
var result = await kernel.InvokePromptAsync(prompt);
Console.WriteLine(result);
```

## ASP.NET Core

`appsettings.json`

```json
{
    "dashScope": {
        "apiKey": "your-key",
        "chatCompletionModelId": "qwen-max",
        "textEmbeddingModelId": "text-embedding-v2"
    }
}
```

`Program.cs`
```csharp
builder.AddTransient<Kernel>(sp => new Kernel(sp));
```

Services

```csharp
public class YourService(Kernel kernel)
{
    public async Task<string> GetCompletionAsync(string prompt)
    {
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var chatResult = await chatCompletionService.GetChatMessageContentAsync(prompt, null, _kernel);
        return chatResult.ToString();
    }
}
```
