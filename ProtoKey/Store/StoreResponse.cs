namespace ProtoKey.Store;

public sealed class StoreResponse
{
    public bool Success { get; init; }
    public int Value { get; init; }
    public IReadOnlyList<string> Keys { get; init; } = Array.Empty<string>();
    public string? Error { get; init; }

    public static StoreResponse Ok() => new() { Success = true };
    public static StoreResponse Ok(int value) => new() { Success = true, Value = value };
    public static StoreResponse Ok(IReadOnlyList<string> keys) => new() { Success = true, Keys = keys };
    public static StoreResponse Fail(string error) => new() { Success = false, Error = error };
}