# Cnblogs.SemanticKernel.Connectors.DashScope
Semantic Kernel Connector to DashScope

## Get started
Add the NuGet package to your project.
```shell 
dotnet add package Cnblogs.SemanticKernel.Connectors.DashScope
```

Add the `dashscope` section to the appsettings.json file.
```json
{
  "dashscope": {
    "modelId": "qwen-max"
  }
}
```
Add the api key into the user-secrets.
```shell
dotnet user-secrets init
dotnet user-secrets set "dashscope:apiKey" "sk-xxx"
```
## Usage
Program.cs
```cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

var builder = Kernel.CreateBuilder();
builder.Services.AddSingleton(GetConfiguration());
builder.AddDashScopeChatCompletion();
var kernel = builder.Build();

var prompt = @"<message role=""user"">Tell me about the Cnblogs</message>";
var result = await kernel.InvokePromptAsync(prompt);
Console.WriteLine(result);

static IConfiguration GetConfiguration()
{
    return new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddUserSecrets<Program>()
        .Build();
}

public partial class Program
{ }
```

