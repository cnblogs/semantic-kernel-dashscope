using Cnblogs.SemanticKernel.Connectors.DashScope;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;

namespace SemanticKernel.DashScope.UnitTest;

public class ServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(InitializationType.ApiKey)]
    [InlineData(InitializationType.Configuration)]
    public void ServiceCollectionExtension_AddChatService_AddTextAndChatService(InitializationType type)
    {
        // Arrange
        var builder = Kernel.CreateBuilder();
        builder.Services.AddLogging();

        // Act
        _ = type switch
        {
            InitializationType.ApiKey => builder.Services.AddDashScopeChatCompletion(Cases.ApiKey, Cases.ModelId),
            InitializationType.Configuration => builder.Services.AddDashScopeChatCompletion(Cases.Configuration),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        // Assert
        var provider = builder.Services.BuildServiceProvider();
        var chat = provider.GetRequiredService<IChatCompletionService>();
        var text = provider.GetRequiredService<ITextGenerationService>();
        chat.Should().BeOfType<DashScopeChatCompletionService>();
        text.Should().BeOfType<DashScopeChatCompletionService>();
    }

    [Theory]
    [InlineData(InitializationType.ApiKey)]
    [InlineData(InitializationType.Configuration)]
    public void ServiceCollectionExtension_AddEmbeddingService_AddEmbeddingService(InitializationType type)
    {
        // Arrange
        var builder = Kernel.CreateBuilder();

        // Act
        _ = type switch
        {
            InitializationType.ApiKey => builder.Services.AddDashScopeTextEmbeddingGeneration(Cases.ApiKey, Cases.ModelId),
            InitializationType.Configuration => builder.Services.AddDashScopeTextEmbeddingGeneration(Cases.Configuration),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

        // Assert
        var provider = builder.Services.BuildServiceProvider();
        var text = provider.GetRequiredService<ITextEmbeddingGenerationService>();
        text.Should().BeOfType<DashScopeTextEmbeddingGenerationService>();
    }

    public enum InitializationType
    {
        ApiKey,
        Configuration
    }
}
