using FluentAssertions;

namespace KernelMemory.DashScope.UnitTests;

public class DashScopeConfigTests
{
    [Theory]
    [InlineData("   ")]
    [InlineData("")]
    public void Validate_NoApiKey_Throw(string apiKey)
    {
        // Arrange
        var config = Cases.DashScopeConfig with { ApiKey = apiKey };

        // Act
        var act = () => config.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>("api key should not be null or whitespace");
    }

    [Fact]
    public void Validate_NegativeTextGenerateTokenCount_Throw()
    {
        // Arrange
        var config = Cases.DashScopeConfig with { TextModelMaxTokenTotal = -1 };

        // Act
        var act = () => config.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>("token count can not be less than 0");
    }

    [Fact]
    public void Validate_NegativeTextEmbeddingTokenCount_Throw()
    {
        // Arrange
        var config = Cases.DashScopeConfig with { EmbeddingModelMaxTokenTotal = -1 };

        // Act
        var act = () => config.EnsureValid();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>("token count can not be less than 0");
    }
}
