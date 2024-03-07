using Cnblogs.DashScope.Sdk;
using Cnblogs.SemanticKernel.Connectors.DashScope;
using FluentAssertions;
using Microsoft.SemanticKernel;
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
        var service = new DashScopeChatCompletionService(Cases.ModelId, dashScopeClient);

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
    public async Task ChatCompletion_CustomModel_SuccessAsync()
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(Task.FromResult(Cases.ChatGenerationResponse));
        var service = new DashScopeChatCompletionService(Cases.ModelId, dashScopeClient);
        var settings = new DashScopePromptExecutionSettings() { ModelId = Cases.ModelIdAlter };

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
        var service = new DashScopeChatCompletionService(Cases.ModelId, dashScopeClient);

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
        var service = new DashScopeChatCompletionService(Cases.ModelId, dashScopeClient);
        var settings = new DashScopePromptExecutionSettings() { ModelId = Cases.ModelIdAlter };

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
            new PromptExecutionSettings { ExtensionData = new Dictionary<string, object>() { { "seed", 1000 } } }
        };
}
