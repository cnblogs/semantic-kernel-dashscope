using System.Diagnostics;
using System.Text;
using SemanticKernel.DashScope.IntegrationTest;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework($"{IntegrationTestFramework.Namespace}.{nameof(IntegrationTestFramework)}", IntegrationTestFramework.Namespace)]

namespace SemanticKernel.DashScope.IntegrationTest;

public class IntegrationTestFramework : XunitTestFramework
{
    public const string Namespace = $"{nameof(SemanticKernel)}.{nameof(DashScope)}.{nameof(IntegrationTest)}";

    public IntegrationTestFramework(IMessageSink messageSink) : base(messageSink)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Trace.Listeners.Add(new ConsoleTraceListener());
    }
}