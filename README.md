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
builder.Services.AddDashScopeChatCompletion("your-api-key", "qwen-max");
var kernel = builder.Build();

var prompt = "<message role=\"user\">Tell me about the Cnblogs</message>";
var response = await kernel.InvokePromptAsync(prompt);
Console.WriteLine(response);
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
builder.Services.AddDashScopeChatCompletion(builder.Configuration);
builder.Services.AddScoped<Kernel>(sp => new Kernel(sp));
```

Services

```csharp
public class YourService(Kernel kernel)
{
    public async Task<string> GetCompletionAsync(string prompt)
    {
        var chatResult = await kernel.InvokePromptAsync(prompt);
        return chatResult.ToString();
    }
}
```
