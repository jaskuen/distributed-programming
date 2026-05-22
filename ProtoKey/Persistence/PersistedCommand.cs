namespace ProtoKey.Persistence;

internal sealed record PersistedCommand(
    string Key,
    int Value,
    TaskCompletionSource? Completion = null)
{
    public string ToDataLine() => $"set {Key} {Value}";

    public static bool TryParse(string line, out PersistedCommand command)
    {
        command = new PersistedCommand(string.Empty, 0);
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length != 3 ||
            !string.Equals(parts[0], "set", StringComparison.OrdinalIgnoreCase) ||
            !ProtoKey.Validation.InputValidator.IsValidKey(parts[1]) ||
            !int.TryParse(parts[2], out int value))
        {
            return false;
        }

        command = new PersistedCommand(parts[1], value);
        return true;
    }
}
