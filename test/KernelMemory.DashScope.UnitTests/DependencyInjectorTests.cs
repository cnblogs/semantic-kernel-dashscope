using Cnblogs.DashScope.Core;
using Cnblogs.KernelMemory.AI.DashScope;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using NSubstitute;

namespace KernelMemory.DashScope.UnitTests;

public class DependencyInjectorTests
{
    [Fact]
    public void TextEmbedding_NormalConfig_Inject()
    {
        // Arrange
        var services = new ServiceCollection();
        var client = Substitute.For<IDashScopeClient>();
        services.AddSingleton(client);

        // Act
        var provider = services.AddDashScopeTextEmbeddingGeneration(Cases.DashScopeConfig).BuildServiceProvider();

        // Assert
        var generator = provider.GetService<ITextEmbeddingGenerator>();
        generator.Should()
            .NotBeNull("the text embedding service should be injected").And
            .BeOfType<DashScopeTextEmbeddingGenerator>("DashScope implementation should be used");
        generator!.MaxTokens.Should().Be(Cases.DashScopeConfig.EmbeddingModelMaxTokenTotal);
    }

    [Fact]
    public void TextEmbedding_InvalidConfig_Throw()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddDashScopeTextEmbeddingGeneration(Cases.InvalidConfig).BuildServiceProvider();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>("config must be valid");
    }

    [Fact]
    public void TextGenerator_NormalConfig_Inject()
    {
        // Arrange
        var services = new ServiceCollection();
        var client = Substitute.For<IDashScopeClient>();
        services.AddSingleton(client);

        // Act
        var provider = services.AddDashScopeTextGeneration(Cases.DashScopeConfig).BuildServiceProvider();

        // Assert
        var generator = provider.GetService<ITextGenerator>();
        generator.Should()
            .NotBeNull("the text embedding service should be injected").And
            .BeOfType<DashScopeTextGenerator>("DashScope implementation should be used");
        generator!.MaxTokenTotal.Should().Be(Cases.DashScopeConfig.TextModelMaxTokenTotal);
    }

    [Fact]
    public void TextGenerator_InvalidConfig_Throw()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.AddDashScopeTextGeneration(Cases.InvalidConfig).BuildServiceProvider();

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>("config must be valid");
    }
}
