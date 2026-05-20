namespace ProtoCli;

internal sealed class CommandResult
{
    public bool Success { get; init; }
    public string Output { get; init; } = string.Empty;
    public IReadOnlyList<string> Keys { get; init; } = Array.Empty<string>();
    public string Error { get; init; } = string.Empty;

    public static CommandResult Ok(string output = "") => new() { Success = true, Output = output };
    public static CommandResult Ok(IReadOnlyList<string> keys) => new() { Success = true, Keys = keys };
    public static CommandResult Fail(string error) => new() { Success = false, Error = error };
}