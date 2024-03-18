using Cnblogs.DashScope.Core;
using Cnblogs.SemanticKernel.Connectors.DashScope;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using NSubstitute;
using NSubstitute.Extensions;

namespace SemanticKernel.DashScope.UnitTest;

public class TextCompletionTests
{
    [Theory]
    [MemberData(nameof(Settings))]
    public async Task GetTextContent_Normal_SuccessAsync(PromptExecutionSettings? settings)
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(Task.FromResult(Cases.TextGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLogger<DashScopeChatCompletionService>.Instance);

        // Act
        var response = await service.GetTextContentsAsync(Cases.Prompt, settings);

        // Assert
        await dashScopeClient.Received().GetTextCompletionAsync(
            Arg.Is<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(
                x => x.Parameters != null
                     && x.Parameters.Seed == (settings == null ? null : 1000)
                     && x.Parameters.IncrementalOutput == false
                     && x.Parameters.ResultFormat == ResultFormats.Text));
        response.Should().BeEquivalentTo([new { Cases.TextGenerationResponse.Output.Text }]);
        response[0].Metadata.Should()
            .Contain(
            [
                new KeyValuePair<string, object?>("Usage", Cases.TextGenerationResponse.Usage),
                new KeyValuePair<string, object?>("RequestId", Cases.TextGenerationResponse.RequestId)
            ]);
    }

    [Fact]
    public async Task GetTextContent_OverrideModelId_SuccessAsync()
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetTextCompletionAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(Task.FromResult(Cases.TextGenerationResponse));
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLogger<DashScopeChatCompletionService>.Instance);
        var settings = new DashScopePromptExecutionSettings { ModelId = Cases.ModelIdAlter };

        // Act
        _ = await service.GetTextContentsAsync(Cases.Prompt, settings);

        // Assert
        await dashScopeClient.Received().GetTextCompletionAsync(
            Arg.Is<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(x => x.Model == Cases.ModelIdAlter));
    }

    [Theory]
    [MemberData(nameof(Settings))]
    public async Task GetTextContentStream_Normal_SuccessAsync(PromptExecutionSettings? settings)
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        var list = new[] { Cases.TextGenerationResponse };
        dashScopeClient.Configure()
            .GetTextCompletionStreamAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(list.ToAsyncEnumerable());
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLogger<DashScopeChatCompletionService>.Instance);

        // Act
        var response = await service.GetStreamingTextContentsAsync(Cases.Prompt, settings).ToListAsync();

        // Assert
        _ = dashScopeClient.Received().GetTextCompletionStreamAsync(
            Arg.Is<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(
                x => x.Parameters != null
                     && x.Parameters.Seed == (settings == null ? null : 1000)
                     && x.Parameters.IncrementalOutput == true
                     && x.Parameters.ResultFormat == ResultFormats.Text));
        response.Should().BeEquivalentTo([new { Cases.TextGenerationResponse.Output.Text }]);
        response[0].Metadata.Should()
            .Contain(
            [
                new KeyValuePair<string, object?>("Usage", Cases.TextGenerationResponse.Usage),
                new KeyValuePair<string, object?>("RequestId", Cases.TextGenerationResponse.RequestId)
            ]);
    }

    [Fact]
    public async Task GetTextContentStream_OverrideModelId_SuccessAsync()
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        var list = new[] { Cases.TextGenerationResponse };
        dashScopeClient.Configure()
            .GetTextCompletionStreamAsync(Arg.Any<ModelRequest<TextGenerationInput, ITextGenerationParameters>>())
            .Returns(list.ToAsyncEnumerable());
        var service = new DashScopeChatCompletionService(
            Cases.ModelId,
            dashScopeClient,
            NullLogger<DashScopeChatCompletionService>.Instance);
        var settings = new PromptExecutionSettings { ModelId = Cases.ModelIdAlter };

        // Act
        _ = await service.GetStreamingTextContentsAsync(Cases.Prompt, settings).ToListAsync();

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
