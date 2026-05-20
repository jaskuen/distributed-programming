using System.Threading.Channels;

namespace ProtoKey.Store;

public sealed class StoreClient(Channel<StoreCommand> commands)
{
    public Task<StoreResponse> SetAsync(string key, int value, CancellationToken cancellationToken)
    {
        TaskCompletionSource<StoreResponse> completion = CreateCompletion(cancellationToken);
        return WriteAsync(new SetCommand(key, value, completion), completion, cancellationToken);
    }

    public Task<StoreResponse> GetAsync(string key, CancellationToken cancellationToken)
    {
        TaskCompletionSource<StoreResponse> completion = CreateCompletion(cancellationToken);
        return WriteAsync(new GetCommand(key, completion), completion, cancellationToken);
    }

    public Task<StoreResponse> KeysAsync(string prefix, CancellationToken cancellationToken)
    {
        TaskCompletionSource<StoreResponse> completion = CreateCompletion(cancellationToken);
        return WriteAsync(new KeysCommand(prefix, completion), completion, cancellationToken);
    }

    private async Task<StoreResponse> WriteAsync(
        StoreCommand command,
        TaskCompletionSource<StoreResponse> completion,
        CancellationToken cancellationToken)
    {
        await commands.Writer.WriteAsync(command, cancellationToken);
        return await completion.Task.WaitAsync(cancellationToken);
    }

    private static TaskCompletionSource<StoreResponse> CreateCompletion(CancellationToken cancellationToken)
    {
        TaskCompletionSource<StoreResponse> completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
        return completion;
    }
}