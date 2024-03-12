using Cnblogs.KernelMemory.AI.DashScope;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
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
        memory.Should().BeOfType<MemoryServerless>();
        provider.GetService<ITextGenerator>().Should().NotBeNull().And.BeOfType<DashScopeTextGenerator>();
        provider.GetService<ITextEmbeddingGenerator>().Should().NotBeNull().And
            .BeOfType<DashScopeTextEmbeddingGenerator>();
    }

    [Fact]
    public void WithDashScope_UseConfigObject_Success()
    {
        // Arrange
        var service = new ServiceCollection();
        var builder = new KernelMemoryBuilder(service);

        // Act
        var memory = builder.WithDashScope(Cases.DashScopeConfig).Build();
        var provider = service.BuildServiceProvider();

        // Assert
        memory.Should().BeOfType<MemoryServerless>();
        provider.GetService<ITextGenerator>().Should().NotBeNull().And.BeOfType<DashScopeTextGenerator>();
        provider.GetService<ITextEmbeddingGenerator>().Should().NotBeNull().And
            .BeOfType<DashScopeTextEmbeddingGenerator>();
    }

    [Fact]
    public void WithDashScope_UseConfiguration_Success()
    {
        // Arrange
        var service = new ServiceCollection();
        var builder = new KernelMemoryBuilder(service);
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(Cases.Configurations).Build();

        // Act
        var memory = builder.WithDashScope(configuration).Build();
        var provider = service.BuildServiceProvider();

        // Assert
        memory.Should().BeOfType<MemoryServerless>();
        provider.GetService<ITextGenerator>().Should().NotBeNull().And.BeOfType<DashScopeTextGenerator>();
        provider.GetService<ITextEmbeddingGenerator>().Should().NotBeNull().And
            .BeOfType<DashScopeTextEmbeddingGenerator>();
    }
}
