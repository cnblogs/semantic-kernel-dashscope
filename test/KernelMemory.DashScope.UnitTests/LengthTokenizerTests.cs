using Cnblogs.KernelMemory.AI.DashScope;
using FluentAssertions;

namespace KernelMemory.DashScope.UnitTests;

public class LengthTokenizerTests
{
    [Fact]
    public void LengthTokenizer_ReturnLength_SuccessAsync()
    {
        // Arrange
        var tokenizer = new LengthTokenizer();

        // Act
        var count = tokenizer.CountTokens(Cases.Text);

        // Assert
        count.Should().Be(Cases.Text.Length);
    }
}
