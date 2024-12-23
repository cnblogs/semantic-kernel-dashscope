using Cnblogs.DashScope.Core;
using Microsoft.KernelMemory.AI;

namespace Cnblogs.KernelMemory.AI.DashScope;

/// <summary>
/// Tokenizer using QWen
/// </summary>
public class QWenTextTokenizer : ITextTokenizer
{
    /// <inheritdoc />
    public int CountTokens(string text)
    {
        return QWenTokenizer.CountTokens(text);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetTokens(string text)
    {
        return QWenTokenizer.Tokenizer.EncodeToTokens(text, out _).Select(x => x.Value).ToList();
    }
}
