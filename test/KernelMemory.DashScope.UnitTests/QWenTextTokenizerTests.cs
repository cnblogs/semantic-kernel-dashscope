using Cnblogs.KernelMemory.AI.DashScope;
using FluentAssertions;

namespace KernelMemory.DashScope.UnitTests;

public class QWenTextTokenizerTests
{
    private const string Text = Cases.Text;
    private static readonly int[] Tokens = Cases.Tokens;

    [Fact]
    public void QWenTokenizer_Encode_Success()
    {
        // Act
        var tokens = new QWenTextTokenizer().GetTokens(Text);

        // Assert
        tokens.Should().BeEquivalentTo(Tokens);
    }
}
