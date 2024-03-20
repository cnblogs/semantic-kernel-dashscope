using Cnblogs.DashScope.Core;
using Cnblogs.SemanticKernel.Connectors.DashScope;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NSubstitute;
using NSubstitute.Extensions;

namespace SemanticKernel.DashScope.UnitTest;

public class ChatCompletionTests
{
    [Theory]
    [MemberData(nameof(Settings))]
    public async Task ChatCompletion_Normal_SuccessAsync(PromptExecutionSettings? settings)
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(Task.FromResult(Cases.ChatGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);

        // Act
        var response = await service.GetChatMessageContentsAsync(Cases.ChatHistory, settings);

        // Assert
        await dashScopeClient.Received().GetTextCompletionAsync(
            Arg.Is<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(
                x => x.Parameters != null
                     && x.Parameters.Seed == (settings == null ? null : 1000)
                     && x.Parameters.IncrementalOutput == false
                     && x.Parameters.ResultFormat == ResultFormats.Message));
        response.Should().BeEquivalentTo([new { Cases.ChatGenerationResponse.Output.Choices![0].Message.Content }]);
        response[0].Metadata.Should()
            .Contain(
            [
                new KeyValuePair<string, object?>("Usage", Cases.ChatGenerationResponse.Usage),
                new KeyValuePair<string, object?>("RequestId", Cases.ChatGenerationResponse.RequestId)
            ]);
    }

    [Fact]
    public async Task ChatCompletion_ToolCalling_SuccessAsync()
    {
        // Arrange
        var functionCallCount = 0;
        var kernel = Kernel.CreateBuilder().Build();
        var function = Cases.NormalFunction(() => functionCallCount++);
        kernel.Plugins.Add(Cases.Plugin(function));
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(Task.FromResult(Cases.ToolCallResponse(function)), Task.FromResult(Cases.ChatGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings =
            new DashScopePromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        var history = new ChatHistory();

        // Act
        var response = await service.GetChatMessageContentsAsync(history, settings, kernel);

        // Assert
        functionCallCount.Should().Be(1);
        response.Should().HaveCount(1); // model response
        history.Should().HaveCount(2); // tool response + model response
    }

    [Fact]
    public async Task ChatCompletion_MaximumToolCallingCount_SuccessAsync()
    {
        // Arrange
        const int maximumAutoInvokeTime = 5;
        const int autoInvokeResponsesCount = 6;
        var functionCallCount = 0;
        var kernel = Kernel.CreateBuilder().Build();
        var function = Cases.NormalFunction(() => functionCallCount++);
        kernel.Plugins.Add(Cases.Plugin(function));
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(
                Task.FromResult(Cases.ToolCallResponse(function)),
                Enumerable.Range(0, autoInvokeResponsesCount - 1)
                    .Select(_ => Task.FromResult(Cases.ToolCallResponse(function))).ToArray());
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings =
            new DashScopePromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        var history = new ChatHistory();

        // Act
        _ = await service.GetChatMessageContentsAsync(history, settings, kernel);

        // Assert
        functionCallCount.Should().Be(maximumAutoInvokeTime, "tool can only be invoked below maximum auto invoke time");
    }

    [Fact]
    public async Task ChatCompletion_ToolTypeIsNotFunction_SkipAsync()
    {
        // Arrange
        const string nonFunctionToolType = "search";
        var functionCallCount = 0;
        var kernel = Kernel.CreateBuilder().Build();
        var function = Cases.NormalFunction(() => functionCallCount++);
        kernel.Plugins.Add(Cases.Plugin(function));
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(
                Task.FromResult(Cases.ErrToolCallResponse([function], toolType: nonFunctionToolType)),
                Task.FromResult(Cases.ChatGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings =
            new DashScopePromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        var history = new ChatHistory();

        // Act
        _ = await service.GetChatMessageContentsAsync(history, settings, kernel);

        // Assert
        functionCallCount.Should().Be(0, "Tool type can only be function");
    }

    [Fact]
    public async Task ChatCompletion_FunctionCallWithMalformedJson_SkipAsync()
    {
        // Arrange
        const string malFormedJson = "invalid json";
        var functionCallCount = 0;
        var kernel = Kernel.CreateBuilder().Build();
        var function = Cases.NormalFunction(() => functionCallCount++);
        kernel.Plugins.Add(Cases.Plugin(function));
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(
                Task.FromResult(Cases.ErrToolCallResponse([function], paramBody: malFormedJson)),
                Task.FromResult(Cases.ChatGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings =
            new DashScopePromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        var history = new ChatHistory();

        // Act
        _ = await service.GetChatMessageContentsAsync(history, settings, kernel);

        // Assert
        functionCallCount.Should().Be(0, "malformed json should be skipped");
        history.Should().HaveCount(2, "error message should be added to chat history");
    }

    [Fact]
    public async Task ChatCompletion_FunctionThrowException_SkipAsync()
    {
        // Arrange
        var functionCallCount = 0;
        var kernel = Kernel.CreateBuilder().Build();
        var function1 = Cases.NormalFunction(() => throw new InvalidOperationException());
        var function2 = Cases.AlterFunction(() => functionCallCount++);
        kernel.Plugins.Add(Cases.Plugin(function1, function2));
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(
                Task.FromResult(Cases.ToolCallResponse(function1, function2)),
                Task.FromResult(Cases.ChatGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings =
            new DashScopePromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        var history = new ChatHistory();

        // Act
        _ = await service.GetChatMessageContentsAsync(history, settings, kernel);

        // Assert
        functionCallCount.Should().Be(1, "interrupted function call should be skipped");
        history.Should().HaveCount(3, "interrupted function call error message should be added to chat history");
    }

    [Fact]
    public async Task ChatCompletion_FunctionDoesNotExists_SkipAsync()
    {
        // Arrange
        var functionCallCount = 0;
        var kernel = Kernel.CreateBuilder().Build();
        var function = Cases.NormalFunction(() => functionCallCount++);
        var plugin = Cases.Plugin(function);

        // not adds function to kernel
        // kernel.Plugins.Add(plugin);
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(
                Task.FromResult(Cases.ToolCallResponse(function)),
                Task.FromResult(Cases.ChatGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings =
            new DashScopePromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.EnableFunctions(plugin.GetFunctionsMetadata(), autoInvoke: true)
            };
        var history = new ChatHistory();

        // Act
        _ = await service.GetChatMessageContentsAsync(history, settings, kernel);

        // Assert
        functionCallCount.Should().Be(0, "Should not call function that not exists in kernel");
    }

    [Fact]
    public async Task ChatCompletion_CallingNotProvidedFunction_SkipAsync()
    {
        // Arrange
        var function1CallCount = 0;
        var function2CallCount = 0;
        var kernel = Kernel.CreateBuilder().Build();
        var function1 = Cases.NormalFunction(() => function1CallCount++);
        var function2 = Cases.AlterFunction(() => function2CallCount++);
        kernel.Plugins.Add(Cases.Plugin(function1, function2));

        var responseCallingFunction2 = Cases.ToolCallResponse(function2);
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(Task.FromResult(responseCallingFunction2));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings =
            new DashScopePromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.EnableFunctions([function1.Metadata], autoInvoke: true)
            };
        var history = new ChatHistory();

        // Act
        _ = await service.GetChatMessageContentsAsync(history, settings, kernel);

        // Assert
        function1CallCount.Should().Be(0, "can not invoke tools that was not provided in request");
        function2CallCount.Should().Be(0, "tools that not presented in response should not be called");
    }

    [Fact]
    public async Task ChatCompletion_CustomModel_SuccessAsync()
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(Task.FromResult(Cases.ChatGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings = new DashScopePromptExecutionSettings { ModelId = Cases.ModelIdAlter };

        // Act
        _ = await service.GetChatMessageContentsAsync(Cases.ChatHistory, settings);

        // Assert
        await dashScopeClient.Received().GetTextCompletionAsync(
            Arg.Is<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(x => x.Model == Cases.ModelIdAlter));
    }

    [Theory]
    [MemberData(nameof(Settings))]
    public async Task ChatCompletionStream_Normal_SuccessAsync(PromptExecutionSettings? settings)
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        var list = new[] { Cases.ChatGenerationResponse };
        dashScopeClient.Configure()
            .GetTextCompletionStreamAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(list.ToAsyncEnumerable());
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);

        // Act
        var response = await service.GetStreamingChatMessageContentsAsync(Cases.ChatHistory, settings).ToListAsync();

        // Assert
        _ = dashScopeClient.Received().GetTextCompletionStreamAsync(
            Arg.Is<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(
                x => x.Parameters != null
                     && x.Parameters.Seed == (settings == null ? null : 1000)
                     && x.Parameters.IncrementalOutput == true
                     && x.Parameters.ResultFormat == ResultFormats.Message));
        response.Should().BeEquivalentTo([new { Cases.ChatGenerationResponse.Output.Choices![0].Message.Content }]);
        response[0].Metadata.Should()
            .Contain(
            [
                new KeyValuePair<string, object?>("Usage", Cases.ChatGenerationResponse.Usage),
                new KeyValuePair<string, object?>("RequestId", Cases.ChatGenerationResponse.RequestId)
            ]);
    }

    [Fact]
    public async Task ChatCompletionStream_CustomModel_SuccessAsync()
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        var list = new[] { Cases.ChatGenerationResponse };
        dashScopeClient.Configure()
            .GetTextCompletionStreamAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(list.ToAsyncEnumerable());
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLoggerFactory.Instance);
        var settings = new DashScopePromptExecutionSettings { ModelId = Cases.ModelIdAlter };

        // Act
        _ = await service.GetStreamingChatMessageContentsAsync(Cases.ChatHistory, settings).ToListAsync();

        // Assert
        _ = dashScopeClient.Received().GetTextCompletionStreamAsync(
            Arg.Is<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(x => x.Model == Cases.ModelIdAlter));
    }

    public static TheoryData<PromptExecutionSettings?> Settings
        => new()
        {
            null,
            new DashScopePromptExecutionSettings { Seed = 1000 },
            new PromptExecutionSettings { ExtensionData = new Dictionary<string, object> { { "seed", 1000 } } }
        };
}
