using Microsoft.DeepDev;
using Microsoft.KernelMemory.AI;

namespace Cnblogs.KernelMemory.AI.DashScope;

/// <summary>
/// Tokenizer using QWen
/// </summary>
public class QWenTokenizer : ITextTokenizer
{
    private static readonly Dictionary<string, int> SpecialTokens =
        ((string[]) ["<|endoftext|>", "<|im_start|>", "<|im_end|>"])
        .Concat(Enumerable.Range(0, 205).Select(x => $"<|extra_{x}|>"))
        .Select((x, i) => new KeyValuePair<string, int>(x, 151643 + i))
        .ToDictionary();
    private static readonly ITokenizer Tokenizer = TokenizerBuilder.CreateTokenizer(
        DashScopeEmbeddedResource.ReadBpeFile(),
        SpecialTokens,
        @"(?i:'s|'t|'re|'ve|'m|'ll|'d)|[^\r\n\p{L}\p{N}]?\p{L}+|\p{N}| ?[^\s\p{L}\p{N}]+[\r\n]*|\s*[\r\n]+|\s+(?!\S)|\s+");

    /// <summary>
    /// Encode text.
    /// </summary>
    /// <param name="text">The text to be encoded.</param>
    /// <returns></returns>
    public static List<int> Encode(string text)
    {
        return Tokenizer.Encode(text, false);
    }

    /// <summary>
    /// Decode tokens.
    /// </summary>
    /// <param name="tokens">The tokens to be decoded</param>
    /// <returns></returns>
    public static string Decode(int[] tokens)
    {
        return Tokenizer.Decode(tokens);
    }

    /// <inheritdoc />
    public int CountTokens(string text)
    {
        return Tokenizer.Encode(text).Count;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetTokens(string text)
    {
        return Tokenizer.Encode(text).Select(x => Tokenizer.Decode([x])).ToList();
    }

    /// <summary>
    /// Count tokens.
    /// </summary>
    /// <param name="text">The text to be tokenized.</param>
    /// <returns></returns>
    public static int CountTokensStatic(string text)
    {
        return Tokenizer.Encode(text).Count;
    }

    /// <summary>
    /// Get tokens
    /// </summary>
    /// <param name="text">The text to tokenizers.</param>
    /// <returns></returns>
    public static IReadOnlyList<string> GetTokensStatic(string text)
    {
        return Tokenizer.Encode(text).Select(x => Tokenizer.Decode([x])).ToList();
    }
}
