using Cnblogs.DashScope.Sdk;
using Cnblogs.SemanticKernel.Connectors.DashScope;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Extensions;

namespace SemanticKernel.DashScope.UnitTest;

public class TextEmbeddingTests
{
    [Fact]
    public async Task TextEmbedding_GetEmbedding_SuccessAsync()
    {
        // Arrange
        var dashScopeClient = Substitute.For<IDashScopeClient>();
        dashScopeClient.Configure()
            .GetEmbeddingsAsync(Arg.Any<ModelRequest<TextEmbeddingInput, ITextEmbeddingParameters>>())
            .Returns(Task.FromResult(Cases.TextEmbeddingResponse));
        var service = new DashScopeTextEmbeddingGenerationService(Cases.ModelId, dashScopeClient);
        var data = new List<string> { Cases.Prompt };

        // Act
        var response = await service.GenerateEmbeddingsAsync(data);

        // Assert
        await dashScopeClient.Received().GetEmbeddingsAsync(
            Arg.Is<ModelRequest<TextEmbeddingInput, ITextEmbeddingParameters>>(
                x => x.Model == Cases.ModelId && x.Input.Texts == data));
        response.Should().NotBeNull();
    }
}
