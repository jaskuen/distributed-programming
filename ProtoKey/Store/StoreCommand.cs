namespace ProtoKey.Store;

public abstract record StoreCommand(TaskCompletionSource<StoreResponse> Completion);

public sealed record SetCommand(
    string Key,
    int Value,
    TaskCompletionSource<StoreResponse> Completion) : StoreCommand(Completion);

public sealed record GetCommand(
    string Key,
    TaskCompletionSource<StoreResponse> Completion) : StoreCommand(Completion);

public sealed record KeysCommand(
    string Prefix,
    TaskCompletionSource<StoreResponse> Completion) : StoreCommand(Completion);