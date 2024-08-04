using Microsoft.KernelMemory.AI;

namespace Cnblogs.KernelMemory.AI.DashScope;

/// <summary>
/// Not a real tokenizer. Return the length of given text as token count, used for apis that limiting the text length instead of token count.
/// </summary>
public class LengthTokenizer : ITextTokenizer
{
    /// <inheritdoc />
    public int CountTokens(string text)
    {
        return text.Length;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetTokens(string text)
    {
        return text.Select(x => $"{x}").ToList();
    }
}
