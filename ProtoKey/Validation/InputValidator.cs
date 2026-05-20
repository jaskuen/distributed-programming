using System.Text.RegularExpressions;

namespace ProtoKey.Validation;

internal sealed partial class InputValidator
{
    public static bool IsValidKey(string? key) =>
        key is { Length: >= 1 and <= 1000 } && KeyRegex().IsMatch(key);

    public static bool IsValidPrefix(string prefix) =>
        prefix.Length <= 1000 && (prefix.Length == 0 || KeyRegex().IsMatch(prefix));

    [GeneratedRegex("^[a-zA-Z0-9_.-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyRegex();
}