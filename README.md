# SemanticKernel.DashScope

Make DashScope work with Semantic Kernel and Kernel Memory.

## Get started with SemanticKernel
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

## ASP.NET Core with KernelMemory support

Install Nuget package `Cnblogs.KernelMemory.AI.DashScope`

Install Nuget package `Microsoft.KernelMemory.Core`

Install Nuget package `Microsoft.KernelMemory.SemanticKernelPlugin`

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
// Kernel Memory stuff
var memory = new KernelMemoryBuilder(builder.Services).WithDashScope(builder.Configuration).Build();
builder.Services.AddSingleton(memory);

// SK stuff
builder.Services.AddDashScopeChatCompletion(builder.Configuration);
builder.Services.AddSingleton(
    sp =>
    {
        var plugins = new KernelPluginCollection();
        plugins.AddFromObject(
            new MemoryPlugin(sp.GetRequiredService<IKernelMemory>(), waitForIngestionToComplete: true),
            "memory");
        return new Kernel(sp, plugins);
    });
```

Services

```csharp
public class YourService(Kernel kernel, IKernelMemory memory)
{
    public async Task<string> GetCompletionAsync(string prompt)
    {
        var chatResult = await kernel.InvokePromptAsync(prompt);
        return chatResult.ToString();
    }

    public async Task ImportDocumentAsync(string filePath, string documentId)
    {
        await memory.ImportDocumentAsync(filePath, documentId);
    }

    public async Task<string> AskMemoryAsync(string question)
    {
        // use memory.ask to query kernel memory
        var skPrompt = """
                       Question to Kernel Memory: {{$input}}

                       Kernel Memory Answer: {{memory.ask $input}}

                       If the answer is empty say 'I don't know' otherwise reply with a preview of the answer, truncated to 15 words.
                       """;

        // you can bundle created functions into a singleton service to reuse them
        var myFunction = kernel.CreateFunctionFromPrompt(skPrompt);
        var result = await myFunction.InvokeAsync(question);
        return result.ToString();
    }
}
```
