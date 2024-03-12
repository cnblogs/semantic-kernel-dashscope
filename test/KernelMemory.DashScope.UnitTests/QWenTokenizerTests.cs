using Cnblogs.KernelMemory.AI.DashScope;
using FluentAssertions;

namespace KernelMemory.DashScope.UnitTests;

public class QWenTokenizerTests
{
    private const string Text = Cases.Text;
    private static readonly int[] Tokens = Cases.Tokens;

    [Fact]
    public void QWenTokenizer_Encode_Success()
    {
        // Act
        var tokens = QWenTokenizer.Encode(Text);

        // Assert
        tokens.Should().BeEquivalentTo(Tokens);
    }

    [Fact]
    public void QWenTokenizer_Decode_Success()
    {
        // Act
        var text = QWenTokenizer.Decode(Tokens);

        // Assert
        text.Should().Be(Text);
    }

    [Fact]
    public void QWenTokenizer_TokenCount_Success()
    {
        // Arrange
        var tokenizer = new QWenTokenizer();

        // Act
        var staticCount = QWenTokenizer.CountTokensStatic(Text);
        var count = tokenizer.CountTokens(Text);

        // Assert
        staticCount.Should().Be(count, "Static count result should be equivalent to non-static count");
        count.Should().Be(Tokens.Length);
    }
}
