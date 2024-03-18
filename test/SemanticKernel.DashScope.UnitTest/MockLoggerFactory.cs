using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;

namespace SemanticKernel.DashScope.UnitTest;

public static class MockLoggerFactory
{
    public static ILogger<T> MockLogger<T>()
    {
        var logger = Substitute.For<ILogger<T>>();
        logger.Configure().IsEnabled(Arg.Any<LogLevel>()).Returns(true);
        return logger;
    }
}
