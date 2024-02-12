using Microsoft.Extensions.Options;

namespace Cnblogs.SemanticKernel.DashScope;

public class DashScopeClientOptions : IOptions<DashScopeClientOptions>
{
    public string ModelId { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;

    public DashScopeClientOptions Value => this;
}