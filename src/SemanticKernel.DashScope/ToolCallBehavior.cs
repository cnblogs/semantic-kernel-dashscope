using System.Text.Json;
using Cnblogs.DashScope.Core;
using Microsoft.SemanticKernel;

namespace Cnblogs.SemanticKernel.Connectors.DashScope;

/// <summary>
/// Represents a behavior for DashScope tool calls.
/// </summary>
public abstract class ToolCallBehavior
{
    /// <summary>
    /// The default maximum number of tool-call auto-invokes that can be made in a single request.
    /// </summary>
    /// <remarks>
    /// After this number of iterations as part of a single user request is reached, auto-invocation
    /// will be disabled (e.g. <see cref="AutoInvokeKernelFunctions"/> will behave like <see cref="EnableKernelFunctions"/>)).
    /// This is a safeguard against possible runaway execution if the model routinely re-requests
    /// the same function over and over. It is currently hardcoded, but in the future it could
    /// be made configurable by the developer. Other configuration is also possible in the future,
    /// such as a delegate on the instance that can be invoked upon function call failure (e.g. failure
    /// to find the requested function, failure to invoke the function, etc.), with behaviors for
    /// what to do in such a case, e.g. respond to the model telling it to try again. With parallel tool call
    /// support, where the model can request multiple tools in a single response, it is significantly
    /// less likely that this limit is reached, as most of the time only a single request is needed.
    /// </remarks>
    private const int DefaultMaximumAutoInvokeAttempts = 5;

    internal ToolCallBehavior(bool autoInvoke)
    {
        MaximumAutoInvokeAttempts = autoInvoke ? DefaultMaximumAutoInvokeAttempts : 0;
    }

    /// <summary>
    /// Options to control tool call result serialization behavior.
    /// </summary>
    public JsonSerializerOptions? ToolCallResultSerializerOptions { get; set; }

    internal int MaximumAutoInvokeAttempts { get; }

    /// <summary>Configures the <paramref name="options"/> with any tools this <see cref="ToolCallBehavior"/> provides.</summary>
    /// <param name="kernel">The <see cref="Kernel"/> used for the operation. This can be queried to determine what tools to provide into the <paramref name="options"/>.</param>
    /// <param name="options">The destination <see cref="DashScopePromptExecutionSettings"/> to configure.</param>
    internal abstract void ConfigureOptions(Kernel? kernel, DashScopePromptExecutionSettings options);

    /// <summary>Gets an instance that will provide the specified list of functions to the model.</summary>
    /// <param name="functions">The functions that should be made available to the model.</param>
    /// <param name="autoInvoke">true to attempt to automatically handle function call requests; otherwise, false.</param>
    /// <returns>
    /// The <see cref="ToolCallBehavior"/> that may be set into <see cref="ToolCallBehavior"/>
    /// to indicate that the specified functions should be made available to the model.
    /// </returns>
    public static ToolCallBehavior EnableFunctions(IEnumerable<KernelFunctionMetadata> functions, bool autoInvoke)
    {
        return new EnabledFunctions(functions, autoInvoke);
    }

    /// <summary>
    /// Gets an instance that will provide all of the <see cref="Kernel"/>'s plugins' function information.
    /// Function call requests from the model will be propagated back to the caller.
    /// </summary>
    /// <remarks>
    /// If no <see cref="Kernel"/> is available, no function information will be provided to the model.
    /// </remarks>
    public static ToolCallBehavior EnableKernelFunctions { get; } = new KernelFunctions(autoInvoke: false);

    /// <summary>
    /// Gets an instance that will both provide all of the <see cref="Kernel"/>'s plugins' function information
    /// to the model and attempt to automatically handle any function call requests.
    /// </summary>
    /// <remarks>
    /// When successful, tool call requests from the model become an implementation detail, with the service
    /// handling invoking any requested functions and supplying the results back to the model.
    /// If no <see cref="Kernel"/> is available, no function information will be provided to the model.
    /// </remarks>
    public static ToolCallBehavior AutoInvokeKernelFunctions { get; } = new KernelFunctions(autoInvoke: true);

    private sealed class EnabledFunctions(IEnumerable<KernelFunctionMetadata> functions, bool autoInvoke = false)
        : ToolCallBehavior(autoInvoke)
    {
        /// <inheritdoc />
        internal override void ConfigureOptions(Kernel? kernel, DashScopePromptExecutionSettings options)
        {
            // If no kernel is provided, we don't have any tools to provide.
            if (kernel is null)
            {
                return;
            }

            // Provide all functions from the kernel.
            if (functions.Any())
            {
                options.Tools = functions
                    .Select(x => new ToolDefinition(ToolTypes.Function, x.ToFunctionDefinition()))
                    .ToList();
            }
        }
    }

    private sealed class KernelFunctions(bool autoInvoke = false) : ToolCallBehavior(autoInvoke)
    {
        /// <inheritdoc />
        internal override void ConfigureOptions(Kernel? kernel, DashScopePromptExecutionSettings options)
        {
            // If no kernel is provided, we don't have any tools to provide.
            if (kernel is null)
            {
                return;
            }

            // Provide all functions from the kernel.
            var functions = kernel.Plugins.GetFunctionsMetadata();
            if (functions.Count > 0)
            {
                options.Tools = functions
                    .Select(x => new ToolDefinition(ToolTypes.Function, x.ToFunctionDefinition()))
                    .ToList();
            }
        }
    }
}
