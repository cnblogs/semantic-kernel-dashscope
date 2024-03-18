using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Cnblogs.DashScope.Core;
using Json.Schema;
using Json.Schema.Generation;
using Microsoft.SemanticKernel;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

/// <summary>
/// Convertors from <see cref="KernelFunctionMetadata"/> to <see cref="FunctionDefinition"/>
/// </summary>
internal static class FunctionDefinitionConvertor
{
    private static readonly KernelJsonSchema DefaultSchemaForTypelessParameter =
        KernelJsonSchema.Parse("{\"type\":\"string\"}");

    private const char FunctionNameSeparator = '-';

    public static FunctionDefinition ToFunctionDefinition(this KernelFunctionMetadata metadata)
    {
        var required = new List<string>();
        var properties = new Dictionary<string, KernelJsonSchema>();
        foreach (var parameter in metadata.Parameters)
        {
            properties.Add(
                parameter.Name,
                parameter.Schema ?? GetDefaultSchemaForTypelessParameter(parameter.Description));
            if (parameter.IsRequired)
            {
                required.Add(parameter.Name);
            }
        }

        var schema = KernelJsonSchema.Parse(
            JsonSerializer.Serialize(
                new
                {
                    type = "object",
                    required,
                    properties
                }));

        var qualifiedName = string.IsNullOrEmpty(metadata.PluginName)
            ? metadata.Name
            : string.Join(FunctionNameSeparator, metadata.PluginName, metadata.Name);
        return new FunctionDefinition(qualifiedName, metadata.Description, schema);
    }

    public static bool TryGetKernelFunctionAndArguments(
        this KernelPluginCollection collection,
        FunctionCall functionCall,
        [NotNullWhen(true)] out KernelFunction? function,
        out KernelArguments? arguments)
    {
        var qualifiedName = functionCall.Name.AsSpan();
        var separatorIndex = qualifiedName.IndexOf(FunctionNameSeparator);
        string? pluginName = null;
        var functionName = functionCall.Name;
        if (separatorIndex > 0)
        {
            pluginName = qualifiedName[..separatorIndex].Trim().ToString();
            functionName = qualifiedName[(separatorIndex + 1)..].Trim().ToString();
        }

        arguments = null;
        if (collection.TryGetFunction(pluginName, functionName, out function) == false)
        {
            return false;
        }

        if (string.IsNullOrEmpty(functionCall.Arguments))
        {
            return true;
        }

        var dic = JsonSerializer.Deserialize<Dictionary<string, object?>>(functionCall.Arguments)!;
        arguments = new KernelArguments();
        foreach (var parameter in dic)
        {
            arguments[parameter.Key] = parameter.Value?.ToString();
        }

        return true;
    }

    private static KernelJsonSchema GetDefaultSchemaForTypelessParameter(string? description)
    {
        // If there's a description, incorporate it.
        if (!string.IsNullOrWhiteSpace(description))
        {
            return KernelJsonSchema.Parse(
                JsonSerializer.Serialize(
                    new JsonSchemaBuilder()
                        .FromType(typeof(string))
                        .Description(description)
                        .Build()));
        }

        // Otherwise, we can use a cached schema for a string with no description.
        return DefaultSchemaForTypelessParameter;
    }
}
