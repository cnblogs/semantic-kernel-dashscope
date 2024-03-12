using Cnblogs.KernelMemory.AI.DashScope;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;

namespace KernelMemory.DashScope.UnitTests;

public class KernelMemoryBuilderExtensionTests
{
    [Fact]
    public void WithDashScopeDefaults_UseDefaults_Success()
    {
        // Arrange
        var service = new ServiceCollection();
        var builder = new KernelMemoryBuilder(service);

        // Act
        var memory = builder.WithDashScopeDefaults(Cases.DashScopeConfig.ApiKey).Build();
        var provider = service.BuildServiceProvider();

        // Assert
        memory.Should().BeOfType<MemoryService>();
        provider.GetService<ITextGenerator>().Should().NotBeNull().And.BeOfType<DashScopeTextGenerator>();
        provider.GetService<ITextEmbeddingGenerator>().Should().NotBeNull().And
            .BeOfType<DashScopeTextEmbeddingGenerator>();
    }

    [Fact]
    public void WithDashScope_UseConfig_Success()
    {
        // Arrange
        var service = new ServiceCollection();
        var builder = new KernelMemoryBuilder(service);

        // Act
        var memory = builder.WithDashScope(Cases.DashScopeConfig).Build();
        var provider = service.BuildServiceProvider();

        // Assert
        memory.Should().BeOfType<MemoryService>();
        provider.GetService<ITextGenerator>().Should().NotBeNull().And.BeOfType<DashScopeTextGenerator>();
        provider.GetService<ITextEmbeddingGenerator>().Should().NotBeNull().And
            .BeOfType<DashScopeTextEmbeddingGenerator>();
    }

    [Fact]
    public void WithDashScopeTextEmbedding_UseConfig_Success()
    {
        // Arrange
        var service = new ServiceCollection();
        var builder = new KernelMemoryBuilder(service);

        // Act
        var memory = builder.WithDashScopeTextEmbeddingGeneration(Cases.DashScopeConfig).Build();
        var provider = service.BuildServiceProvider();

        // Assert
        memory.Should().BeOfType<MemoryService>();
        provider.GetService<ITextEmbeddingGenerator>().Should().NotBeNull().And
            .BeOfType<DashScopeTextEmbeddingGenerator>();
    }

    [Fact]
    public void WithDashScopeTextGenerator_UseConfig_Success()
    {
        // Arrange
        var service = new ServiceCollection();
        var builder = new KernelMemoryBuilder(service);

        // Act
        var memory = builder.WithDashScopeTextGeneration(Cases.DashScopeConfig).Build();
        var provider = service.BuildServiceProvider();

        // Assert
        memory.Should().BeOfType<MemoryService>();
        provider.GetService<ITextGenerator>().Should().NotBeNull().And.BeOfType<DashScopeTextGenerator>();
    }
}
