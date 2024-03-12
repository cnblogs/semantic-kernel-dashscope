using Cnblogs.DashScope.Sdk;
using Cnblogs.KernelMemory.AI.DashScope;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Extensions;

namespace KernelMemory.DashScope.UnitTests;

public class DashScopeTextEmbeddingGeneratorTests
{
    [Fact]
    public async Task EmbeddingGenerator_GenerateEmbeddings_GenerateAsync()
    {
        // Arrange
        ModelRequest<TextEmbeddingInput, ITextEmbeddingParameters>? captured = null;
        var client = Substitute.For<IDashScopeClient>();
        client.Configure().GetEmbeddingsAsync(
            Arg.Do<ModelRequest<TextEmbeddingInput, ITextEmbeddingParameters>>(x => captured = x))
            .Returns(Cases.TextEmbeddingResponse);
        var generator = new DashScopeTextEmbeddingGenerator(client, Cases.ModelId);

        // Act
        var embeddings = await generator.GenerateEmbeddingAsync(Cases.Text);

        // Assert
        embeddings.Data.ToArray().Should().BeEquivalentTo(Cases.Embeddings);
        captured!.Parameters.Should().BeNull("no parameter suitable for kernel memory");
        captured!.Input.Texts.Should().BeEquivalentTo([Cases.Text], "input text should be respected");
    }

    [Fact]
    public void EmbeddingGenerator_NullTokenizer_UseLengthTokenizer()
    {
        // Arrange
        var client = Substitute.For<IDashScopeClient>();
        var generator = new DashScopeTextEmbeddingGenerator(client, Cases.ModelId, tokenizer: null);

        // Act
        var count = generator.CountTokens(Cases.Text);

        // Assert
        count.Should().Be(Cases.Text.Length, "Length tokenizer will be used if no tokenizer is given");
    }

    [Fact]
    public void EmbeddingGenerator_SpecifyTokenizer_UseGivenTokenizer()
    {
        // Arrange
        var client = Substitute.For<IDashScopeClient>();
        var generator = new DashScopeTextEmbeddingGenerator(client, Cases.ModelId, tokenizer: new QWenTokenizer());

        // Act
        var count = generator.CountTokens(Cases.Text);

        // Assert
        count.Should().Be(Cases.Tokens.Length, "If given, tokenizer from constructor should be used for count tokens");
    }

    [Fact]
    public void EmbeddingGenerator_SpecifyMaxToken_SetMaxToken()
    {
        // Arrange
        const int maxToken = 1000;
        var client = Substitute.For<IDashScopeClient>();
        var generator = new DashScopeTextEmbeddingGenerator(client, Cases.ModelId, null, maxToken);

        // Act
        var count = generator.MaxTokens;

        // Assert
        count.Should().Be(maxToken, "max token specified in constructor should be respected");
    }
}
