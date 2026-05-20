using System.Threading.Channels;
using ProtoKey.Persistence;

namespace ProtoKey.Store;

internal sealed class StoreWorker(
    Channel<StoreCommand> commands,
    Channel<PersistedCommand> persistedCommands,
    IWebHostEnvironment environment,
    ILogger<StoreWorker> logger) : BackgroundService
{
    private readonly Dictionary<string, int> storage = new(StringComparer.Ordinal);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await LoadSnapshotAsync(stoppingToken);

        await foreach (StoreCommand command in commands.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                switch (command)
                {
                    case SetCommand setCommand:
                        storage[setCommand.Key] = setCommand.Value;
                        await persistedCommands.Writer.WriteAsync(
                            new PersistedCommand(setCommand.Key, setCommand.Value),
                            stoppingToken);
                        setCommand.Completion.TrySetResult(StoreResponse.Ok());
                        break;

                    case GetCommand getCommand:
                        storage.TryGetValue(getCommand.Key, out int value);
                        getCommand.Completion.TrySetResult(StoreResponse.Ok(value));
                        break;

                    case KeysCommand keysCommand:
                        string[] keys = storage.Keys
                            .Where(key => key.StartsWith(keysCommand.Prefix, StringComparison.Ordinal))
                            .Order(StringComparer.Ordinal)
                            .ToArray();
                        keysCommand.Completion.TrySetResult(StoreResponse.Ok(keys));
                        break;

                    default:
                        command.Completion.TrySetResult(StoreResponse.Fail("Unknown command."));
                        break;
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process store command.");
                command.Completion.TrySetException(exception);
            }
        }
    }

    private async Task LoadSnapshotAsync(CancellationToken cancellationToken)
    {
        string path = PersistencePaths.DataFile(environment.ContentRootPath);

        if (!File.Exists(path))
        {
            return;
        }

        string[] lines = await File.ReadAllLinesAsync(path, cancellationToken);

        foreach (string line in lines)
        {
            if (PersistedCommand.TryParse(line, out PersistedCommand? command))
            {
                storage[command.Key] = command.Value;
            }
            else
            {
                logger.LogWarning("Ignored invalid data line: {Line}", line);
            }
        }
    }
}