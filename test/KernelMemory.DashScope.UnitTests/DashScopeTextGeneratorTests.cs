using Cnblogs.DashScope.Core;
using Cnblogs.KernelMemory.AI.DashScope;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.Extensions;

namespace KernelMemory.DashScope.UnitTests;

public class DashScopeTextGeneratorTests
{
    [Fact]
    public async Task TextGenerator_GenerateText_IncrementalGenerateAsync()
    {
        // Arrange
        var list = new[] { Cases.TextGenerationResponse };
        var client = Substitute.For<IDashScopeClient>();
        ModelRequest<TextGenerationInput, ITextGenerationParameters>? captured = null;
        client.Configure()
            .GetTextCompletionStreamAsync(
                Arg.Do<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(x => captured = x))
            .Returns(list.ToAsyncEnumerable());
        var generator = new DashScopeTextGenerator(client, Cases.ModelId, new NullLoggerFactory());

        // Act
        var response = await generator.GenerateTextAsync(Cases.Text, Cases.TextGenerationOptions).ToListAsync();

        // Assert
        response[0].Should().BeSameAs(
            Cases.TextGenerationResponse.Output.Text,
            "generated text should mapped from output.text");
        captured.Should().BeEquivalentTo(
            new { Parameters = Cases.TextGenerationParameters },
            "text options should be mapped to text generation parameters correctly");
    }

    [Fact]
    public async Task TextGenerator_DefaultsToZero_MapZeroToNullAsync()
    {
        // Arrange
        var list = new[] { Cases.TextGenerationResponse };
        var client = Substitute.For<IDashScopeClient>();
        ModelRequest<TextGenerationInput, ITextGenerationParameters>? captured = null;
        client.Configure()
            .GetTextCompletionStreamAsync(
                Arg.Do<ModelRequest<TextGenerationInput, ITextGenerationParameters>>(x => captured = x))
            .Returns(list.ToAsyncEnumerable());
        var generator = new DashScopeTextGenerator(client, Cases.ModelId, new NullLoggerFactory());

        // Act
        var response = await generator.GenerateTextAsync(Cases.Text, Cases.TextGenerationOptions).ToListAsync();

        // Assert
        response[0].Should().BeSameAs(
            Cases.TextGenerationResponse.Output.Text,
            "generated text should mapped from output.text");
        captured.Should().BeEquivalentTo(
            new { Parameters = Cases.TextGenerationParameters },
            "text options should be mapped to text generation parameters correctly");
    }

    [Fact]
    public void TextGenerator_NullTokenizer_UseQWenTokenizer()
    {
        // Arrange
        var client = Substitute.For<IDashScopeClient>();
        var generator = new DashScopeTextGenerator(client, Cases.ModelId, new NullLoggerFactory(), tokenizer: null);

        // Act
        var count = generator.CountTokens(Cases.Text);

        // Assert
        count.Should().Be(Cases.Tokens.Length, "QWen tokenizer will be used if no tokenizer is given");
    }

    [Fact]
    public void TextGenerator_SpecifyTokenizer_UseGivenTokenizer()
    {
        // Arrange
        var client = Substitute.For<IDashScopeClient>();
        var generator = new DashScopeTextGenerator(client, Cases.ModelId, new NullLoggerFactory(), tokenizer: new LengthTokenizer());

        // Act
        var count = generator.CountTokens(Cases.Text);

        // Assert
        count.Should().Be(Cases.Text.Length, "If given, tokenizer from constructor should be used for count tokens");
    }

    [Fact]
    public void TextGenerator_SpecifyMaxToken_SetMaxToken()
    {
        // Arrange
        const int maxToken = 1000;
        var client = Substitute.For<IDashScopeClient>();
        var generator = new DashScopeTextGenerator(client, Cases.ModelId, new NullLoggerFactory(), null, maxToken);

        // Act
        var count = generator.MaxTokenTotal;

        // Assert
        count.Should().Be(maxToken, "max token specified in constructor should be respected");
    }
}
