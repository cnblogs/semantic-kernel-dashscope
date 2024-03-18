using Cnblogs.DashScope.Core;
using Microsoft.SemanticKernel;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

/// <summary>
/// Function definition for model to use.
/// </summary>
public record FunctionDefinition : IFunctionDefinition
{
    /// <summary>
    /// Creates a new function definition.
    /// </summary>
    /// <param name="Name">The name of this function.</param>
    /// <param name="Description">The description of this function.</param>
    /// <param name="Parameters">Parameter map of this function.</param>
    public FunctionDefinition(string Name, string Description, KernelJsonSchema? Parameters)
    {
        this.Description = Description;
        this.Name = Name;
        this.Parameters = Parameters;
    }

    /// <inheritdoc />
    public string Name { get; init; }

    /// <inheritdoc />
    public string Description { get; init; }

    /// <inheritdoc />
    public object? Parameters { get; init; }
}
